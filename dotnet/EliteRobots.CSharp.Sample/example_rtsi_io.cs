using EliteRobots.CSharp;

internal static class RtsiIOExample
{
    internal static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- rtsi_io <robot-ip>");
            return;
        }

        var ip = args[1];

        var outputRecipe = new[]
        {
            "timestamp",
            "actual_joint_positions",
            "actual_joint_speeds",
            "actual_joint_torques",
            "actual_joint_current",
            "joint_temperatures",
            "actual_TCP_pose",
            "actual_TCP_speed",
            "actual_TCP_force",
            "target_joint_positions",
            "target_joint_speeds",
            "target_TCP_pose",
            "target_TCP_speed",
            "robot_mode",
            "joint_mode",
            "safety_status",
            "runtime_state",
            "robot_status_bits",
            "safety_status_bits",
            "actual_digital_input_bits",
            "actual_digital_output_bits",
            "analog_io_types",
            "standard_analog_input0",
            "standard_analog_input1",
            "standard_analog_output0",
            "standard_analog_output1",
            "speed_scaling",
            "target_speed_fraction",
            "actual_robot_voltage",
            "actual_robot_current",
            "io_current",
            "tool_mode",
            "tool_analog_input_types",
            "tool_analog_output_types",
            "tool_analog_input",
            "tool_analog_output",
            "tool_output_voltage",
            "tool_output_current",
            "tool_temperature",
            "tool_digital_mode",
            "tool_digital0_mode",
            "tool_digital1_mode",
            "tool_digital2_mode",
            "tool_digital3_mode",
            "elbow_position",
            "elbow_velocity",
            "input_bit_registers0_to_31",
            "input_bit_registers32_to_63",
            "output_bit_registers0_to_31",
            "output_bit_registers32_to_63",
        };

        var inputRecipe = new[]
        {
            "speed_slider_mask",
            "speed_slider_fraction",
            "standard_digital_output_mask",
            "standard_digital_output",
            "configurable_digital_output_mask",
            "configurable_digital_output",
            "standard_analog_output_mask",
            "standard_analog_output_type",
            "standard_analog_output_0",
            "standard_analog_output_1",
            "external_force_torque",
            "tool_digital_output_mask",
            "tool_digital_output",
        };

        using var io = new EliteRtsiIOClient(outputRecipe, inputRecipe, 50);

        RunStep("connect", () => io.connect(ip));
        RunStep("isConnected", io.isConnected);
        RunStep("isStarted", io.isStarted);
        RunStep("getControllerVersion", () => io.getControllerVersion().ToString());

        RunStep("setSpeedScaling", () => io.setSpeedScaling(0.5));
        RunStep("setStandardDigital", () => io.setStandardDigital(0, true));
        RunStep("setConfigureDigital", () => io.setConfigureDigital(0, false));
        RunStep("setAnalogOutputVoltage", () => io.setAnalogOutputVoltage(0, 1.0));
        RunStep("setAnalogOutputCurrent", () => io.setAnalogOutputCurrent(1, 0.01));
        RunStep("setExternalForceTorque", () => io.setExternalForceTorque(new double[6]));
        RunStep("setToolDigitalOutput", () => io.setToolDigitalOutput(0, true));

        RunStep("getTimestamp", io.getTimestamp);
        RunStep("getRobotMode", () => io.getRobotMode().ToString());
        RunStep("getJointMode", () => $"[{string.Join(", ", io.getJointMode())}]");
        RunStep("getSafetyStatus", () => io.getSafetyStatus().ToString());
        RunStep("getRobotStatus", io.getRobotStatus);
        RunStep("getRuntimeState", () => io.getRuntimeState().ToString());

        RunStep("getActualTCPPose", () => $"[{string.Join(", ", io.getActualTCPPose())}]");
        RunStep("getActualTCPVelocity", () => $"[{string.Join(", ", io.getActualTCPVelocity())}]");
        RunStep("getActualTCPForce", () => $"[{string.Join(", ", io.getActualTCPForce())}]");
        RunStep("getTargetTCPPose", () => $"[{string.Join(", ", io.getTargetTCPPose())}]");
        RunStep("getTargetTCPVelocity", () => $"[{string.Join(", ", io.getTargetTCPVelocity())}]");

        RunStep("getTargetJointPositions", () => $"[{string.Join(", ", io.getTargetJointPositions())}]");
        RunStep("getTargetJointVelocity", () => $"[{string.Join(", ", io.getTargetJointVelocity())}]");
        RunStep("getActualJointPositions", () => $"[{string.Join(", ", io.getActualJointPositions())}]");
        RunStep("getActualJointVelocity", () => $"[{string.Join(", ", io.getActualJointVelocity())}]");
        RunStep("getActualJointTorques", () => $"[{string.Join(", ", io.getActualJointTorques())}]");
        RunStep("getActualJointCurrent", () => $"[{string.Join(", ", io.getActualJointCurrent())}]");
        RunStep("getActualJointTemperatures", () => $"[{string.Join(", ", io.getActualJointTemperatures())}]");

        RunStep("getDigitalInputBits", io.getDigitalInputBits);
        RunStep("getDigitalOutputBits", io.getDigitalOutputBits);
        RunStep("getAnalogInput(0)", () => io.getAnalogInput(0));
        RunStep("getAnalogOutput(0)", () => io.getAnalogOutput(0));
        RunStep("getAnalogIOTypes", io.getAnalogIOTypes);
        RunStep("getIOCurrent", io.getIOCurrent);

        RunStep("getToolMode", () => io.getToolMode().ToString());
        RunStep("getToolAnalogInput", io.getToolAnalogInput);
        RunStep("getToolAnalogOutput", io.getToolAnalogOutput);
        RunStep("getToolAnalogInputType", io.getToolAnalogInputType);
        RunStep("getToolAnalogOutputType", io.getToolAnalogOutputType);
        RunStep("getToolOutputVoltage", io.getToolOutputVoltage);
        RunStep("getToolOutputCurrent", io.getToolOutputCurrent);
        RunStep("getToolOutputTemperature", io.getToolOutputTemperature);
        RunStep("getToolDigitalMode", () => io.getToolDigitalMode().ToString());
        RunStep("getToolDigitalOutputMode(0)", () => io.getToolDigitalOutputMode(0).ToString());

        RunStep("getInBoolRegister(64)", () => io.getInBoolRegister(64));
        RunStep("getOutBoolRegister(64)", () => io.getOutBoolRegister(64));
        RunStep("getInIntRegister(24)", () => io.getInIntRegister(24));
        RunStep("getOutIntRegister(24)", () => io.getOutIntRegister(24));
        RunStep("getInDoubleRegister(24)", () => io.getInDoubleRegister(24));
        RunStep("getOutDoubleRegister(24)", () => io.getOutDoubleRegister(24));

        RunStep("getRecipeValue(timestamp)", () =>
        {
            return io.getRecipeValue("timestamp", out double t) ? t : -1;
        });
        RunStep("setInputRecipeValue(speed_slider_mask)", () => io.setInputRecipeValue("speed_slider_mask", 1U));

        RunStep("disconnect", () =>
        {
            io.disconnect();
            return "ok";
        });
    }

    private static void RunStep<T>(string name, Func<T> call)
    {
        try
        {
            var value = call();
            Console.WriteLine($"{name}: {value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name}: FAILED - {ex.Message}");
        }
    }
}
