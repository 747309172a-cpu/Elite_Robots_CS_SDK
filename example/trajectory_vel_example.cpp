// SPDX-License-Identifier: MIT
// Copyright (c) 2025, Elite Robots.
#include <Elite/DashboardClient.hpp>
#include <Elite/DataType.hpp>
#include <Elite/EliteDriver.hpp>
#include <Elite/Log.hpp>
#include <Elite/RtsiIOInterface.hpp>

#include <boost/program_options.hpp>
#include <cmath>
#include <future>
#include <iostream>
#include <memory>
#include <thread>

using namespace ELITE;
namespace po = boost::program_options;

class TrajectoryVelocityControl {
   private:
    std::unique_ptr<EliteDriver> driver_;
    std::unique_ptr<DashboardClient> dashboard_;
    EliteDriverConfig config_;

   public:
    explicit TrajectoryVelocityControl(const EliteDriverConfig& config) {
        config_ = config;
        driver_ = std::make_unique<EliteDriver>(config);
        dashboard_ = std::make_unique<DashboardClient>();

        ELITE_LOG_INFO("Connecting to the dashboard");
        if (!dashboard_->connect(config.robot_ip)) {
            ELITE_LOG_FATAL("Failed to connect to the dashboard.");
            throw std::runtime_error("Failed to connect to the dashboard.");
        }
        ELITE_LOG_INFO("Successfully connected to the dashboard");
    }

    ~TrajectoryVelocityControl() {
        if (dashboard_) {
            dashboard_->disconnect();
        }
        if (driver_) {
            driver_->stopControl();
        }
    }

    bool startControl() {
        ELITE_LOG_INFO("Start powering on...");
        if (!dashboard_->powerOn()) {
            ELITE_LOG_FATAL("Power-on failed");
            return false;
        }
        ELITE_LOG_INFO("Power-on succeeded");

        ELITE_LOG_INFO("Start releasing brake...");
        if (!dashboard_->brakeRelease()) {
            ELITE_LOG_FATAL("Brake release failed");
            return false;
        }
        ELITE_LOG_INFO("Brake released");

        if (config_.headless_mode) {
            if (!driver_->isRobotConnected() && !driver_->sendExternalControlScript()) {
                ELITE_LOG_FATAL("Fail to send external control script");
                return false;
            }
        } else if (!dashboard_->playProgram()) {
            ELITE_LOG_FATAL("Fail to play program");
            return false;
        }

        ELITE_LOG_INFO("Wait external control script run...");
        while (!driver_->isRobotConnected()) {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }
        ELITE_LOG_INFO("External control script is running");
        return true;
    }

    bool moveTrajectory(const std::vector<vector6d_t>& target_points, float blend_radius, bool is_cartesian, float speed,
                        float acceleration) {
        std::promise<TrajectoryMotionResult> move_done_promise;
        driver_->setTrajectoryResultCallback([&](TrajectoryMotionResult result) { move_done_promise.set_value(result); });

        ELITE_LOG_INFO("Trajectory motion start");
        if (!driver_->writeTrajectoryControlAction(ELITE::TrajectoryControlAction::START, target_points.size(), 200)) {
            ELITE_LOG_ERROR("Failed to start trajectory motion");
            return false;
        }

        for (const auto& point : target_points) {
            if (!driver_->writeTrajectoryPoint(point, blend_radius, is_cartesian, speed, acceleration)) {
                ELITE_LOG_ERROR("Failed to write trajectory point");
                return false;
            }
            if (!driver_->writeTrajectoryControlAction(ELITE::TrajectoryControlAction::NOOP, 0, 200)) {
                ELITE_LOG_ERROR("Failed to send NOOP command");
                return false;
            }
        }

        std::future<TrajectoryMotionResult> move_done_future = move_done_promise.get_future();
        while (move_done_future.wait_for(std::chrono::milliseconds(50)) != std::future_status::ready) {
            if (!driver_->writeTrajectoryControlAction(ELITE::TrajectoryControlAction::NOOP, 0, 200)) {
                ELITE_LOG_ERROR("Failed to send NOOP command");
                return false;
            }
        }

        auto result = move_done_future.get();
        ELITE_LOG_INFO("Trajectory motion completed with result: %d", result);

        if (!driver_->writeIdle(0)) {
            ELITE_LOG_ERROR("Failed to write idle command");
            return false;
        }

        return result == TrajectoryMotionResult::SUCCESS;
    }

    bool moveToJointTarget(const vector6d_t& point, float speed, float acceleration) {
        return moveTrajectory({point}, 0.0f, false, speed, acceleration);
    }

    bool moveAlongBaseZAndMonitorVelocity(RtsiIOInterface& rtsi_client, float z_offset, float speed, float acceleration) {
        vector6d_t target_pose = rtsi_client.getActualTCPPose();
        target_pose[2] += z_offset;

        std::promise<TrajectoryMotionResult> move_done_promise;
        driver_->setTrajectoryResultCallback([&](TrajectoryMotionResult result) { move_done_promise.set_value(result); });
        driver_->setTrajectoryFeedbackCallback([&](const TrajectoryMotionFeedback& feedback) {
            if (feedback.message_type == TrajectoryFeedbackMessageType::ACTIVE_POINT) {
                ELITE_LOG_INFO("Trajectory point %d/%d is active", feedback.point_index + 1, feedback.total_points);
            } else if (feedback.message_type == TrajectoryFeedbackMessageType::POINT_DONE) {
                ELITE_LOG_INFO("Trajectory point %d/%d is done", feedback.point_index + 1, feedback.total_points);
            } else if (feedback.message_type == TrajectoryFeedbackMessageType::RESULT) {
                ELITE_LOG_INFO("Trajectory result frame received: %d", feedback.result);
            }
        });

        ELITE_LOG_INFO("Start base-z trajectory. z offset: %lf m, target speed: %lf m/s, target acceleration: %lf m/s^2",
                       z_offset, speed, acceleration);

        if (!driver_->writeTrajectoryControlAction(ELITE::TrajectoryControlAction::START, 1, 200)) {
            ELITE_LOG_ERROR("Failed to start trajectory motion");
            return false;
        }
        if (!driver_->writeTrajectoryPoint(target_pose, 0.0f, true, speed, acceleration)) {
            ELITE_LOG_ERROR("Failed to write cartesian trajectory point");
            return false;
        }
        if (!driver_->writeTrajectoryControlAction(ELITE::TrajectoryControlAction::NOOP, 0, 200)) {
            ELITE_LOG_ERROR("Failed to send initial NOOP command");
            return false;
        }

        std::future<TrajectoryMotionResult> move_done_future = move_done_promise.get_future();
        while (move_done_future.wait_for(std::chrono::milliseconds(50)) != std::future_status::ready) {
            vector6d_t tcp_velocity = rtsi_client.getActualTCPVelocity();
            double linear_speed =
                std::sqrt(tcp_velocity[0] * tcp_velocity[0] + tcp_velocity[1] * tcp_velocity[1] + tcp_velocity[2] * tcp_velocity[2]);
            ELITE_LOG_INFO(
                "Actual TCP velocity [vx, vy, vz, wx, wy, wz] = [%lf, %lf, %lf, %lf, %lf, %lf], linear speed = %lf m/s",
                tcp_velocity[0], tcp_velocity[1], tcp_velocity[2], tcp_velocity[3], tcp_velocity[4], tcp_velocity[5], linear_speed);

            if (!driver_->writeTrajectoryControlAction(ELITE::TrajectoryControlAction::NOOP, 0, 200)) {
                ELITE_LOG_ERROR("Failed to send NOOP command");
                return false;
            }
        }

        auto result = move_done_future.get();
        vector6d_t final_tcp_velocity = rtsi_client.getActualTCPVelocity();
        vector6d_t final_pose = rtsi_client.getActualTCPPose();
        ELITE_LOG_INFO("Final TCP velocity [vx, vy, vz, wx, wy, wz] = [%lf, %lf, %lf, %lf, %lf, %lf]",
                       final_tcp_velocity[0], final_tcp_velocity[1], final_tcp_velocity[2], final_tcp_velocity[3],
                       final_tcp_velocity[4], final_tcp_velocity[5]);
        ELITE_LOG_INFO("Final TCP pose [x, y, z, rx, ry, rz] = [%lf, %lf, %lf, %lf, %lf, %lf]", final_pose[0], final_pose[1],
                       final_pose[2], final_pose[3], final_pose[4], final_pose[5]);

        if (!driver_->writeIdle(0)) {
            ELITE_LOG_ERROR("Failed to write idle command");
            return false;
        }

        return result == TrajectoryMotionResult::SUCCESS;
    }
};

int main(int argc, const char** argv) {
    constexpr float kJointSpeed = 0.5f;
    constexpr float kJointAcceleration = 0.8f;
    constexpr float kCartesianSpeed = 0.15f;
    constexpr float kCartesianAcceleration = 0.3f;
    constexpr float kZOffset = -0.2f;

    EliteDriverConfig config;
    po::options_description desc(
        "Usage:\n"
        "\t./trajectory_vel_example <--robot-ip=ip> [--local-ip=\"\"] [--use-headless-mode=true]\n"
        "Parameters:");
    desc.add_options()("help,h", "Print help message")("robot-ip", po::value<std::string>(&config.robot_ip)->required(),
                                                       "\tRequired. IP address of the robot.")(
        "use-headless-mode", po::value<bool>(&config.headless_mode)->required()->implicit_value(true),
        "\tRequired. Use headless mode.")("local-ip", po::value<std::string>(&config.local_ip)->default_value(""),
                                          "\tOptional. IP address of the local network interface.");

    po::variables_map vm;
    try {
        po::store(po::parse_command_line(argc, argv, desc), vm);

        if (vm.count("help")) {
            std::cout << desc << std::endl;
            return 0;
        }

        po::notify(vm);
    } catch (const po::error& e) {
        std::cerr << "Argument error: " << e.what() << "\n\n";
        std::cerr << desc << "\n";
        return 1;
    }

    if (config.headless_mode) {
        ELITE_LOG_WARN("Use headless mode. Please ensure the robot is not in local mode.");
    } else {
        ELITE_LOG_WARN(
            "It needs to be correctly configured, and the External Control plugin should be inserted into the task tree.");
    }

    config.script_file_path = "external_control.script";
    std::unique_ptr<TrajectoryVelocityControl> trajectory_control = std::make_unique<TrajectoryVelocityControl>(config);
    std::unique_ptr<RtsiIOInterface> rtsi_client = std::make_unique<RtsiIOInterface>("output_recipe.txt", "input_recipe.txt", 250);

    ELITE_LOG_INFO("Connecting to the RTSI");
    if (!rtsi_client->connect(config.robot_ip)) {
        ELITE_LOG_FATAL("Fail to connect or config to the RTSI.");
        return 1;
    }
    ELITE_LOG_INFO("Successfully connected to the RTSI");

    ELITE_LOG_INFO("Starting trajectory control...");
    if (!trajectory_control->startControl()) {
        ELITE_LOG_FATAL("Failed to start trajectory control.");
        return 1;
    }
    ELITE_LOG_INFO("Trajectory control started");

    vector6d_t actual_joints = rtsi_client->getActualJointPositions();
    actual_joints[3] = -1.57;
    ELITE_LOG_INFO("Moving joints to target: [%lf, %lf, %lf, %lf, %lf, %lf]", actual_joints[0], actual_joints[1], actual_joints[2],
                   actual_joints[3], actual_joints[4], actual_joints[5]);
    if (!trajectory_control->moveToJointTarget(actual_joints, kJointSpeed, kJointAcceleration)) {
        ELITE_LOG_FATAL("Failed to move joints to target.");
        return 1;
    }
    ELITE_LOG_INFO("Joints moved to target");

    vector6d_t start_pose = rtsi_client->getActualTCPPose();
    ELITE_LOG_INFO("Start TCP pose [x, y, z, rx, ry, rz] = [%lf, %lf, %lf, %lf, %lf, %lf]", start_pose[0], start_pose[1],
                   start_pose[2], start_pose[3], start_pose[4], start_pose[5]);
    ELITE_LOG_INFO("Expected TCP velocity behavior: vz should approach %lf m/s, other linear/angular components should stay near zero",
                   kCartesianSpeed * (kZOffset >= 0.0f ? 1.0f : -1.0f));

    if (!trajectory_control->moveAlongBaseZAndMonitorVelocity(*rtsi_client, kZOffset, kCartesianSpeed, kCartesianAcceleration)) {
        ELITE_LOG_FATAL("Failed to execute z-axis velocity verification trajectory.");
        return 1;
    }
    ELITE_LOG_INFO("Z-axis trajectory completed");

    return 0;
}
