// SPDX-License-Identifier: MIT
// Copyright (c) 2026, Elite Robots.
//
// C ABI wrapper for source/Rtsi module.
#ifndef __ELITE_RTSI_C_H__
#define __ELITE_RTSI_C_H__

#include <EliteC/elite_c_types.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct elite_rtsi_client_handle_t elite_rtsi_client_handle_t;
typedef struct elite_rtsi_recipe_handle_t elite_rtsi_recipe_handle_t;
typedef struct elite_rtsi_io_handle_t elite_rtsi_io_handle_t;

typedef struct elite_version_info_t {
    uint32_t major;
    uint32_t minor;
    uint32_t bugfix;
    uint32_t build;
} elite_version_info_t;

ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_create(elite_rtsi_client_handle_t** out_handle);
ELITE_C_EXPORT void elite_rtsi_client_destroy(elite_rtsi_client_handle_t* handle);

ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_connect(elite_rtsi_client_handle_t* handle, const char* ip, int32_t port);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_disconnect(elite_rtsi_client_handle_t* handle);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_negotiate_protocol_version(elite_rtsi_client_handle_t* handle,
                                                                              uint16_t version, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_get_controller_version(elite_rtsi_client_handle_t* handle,
                                                                         elite_version_info_t* out_version);

// recipe_csv format: "actual_q,actual_qd,target_q"
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_setup_output_recipe(elite_rtsi_client_handle_t* handle,
                                                                      const char* recipe_csv, double frequency,
                                                                      elite_rtsi_recipe_handle_t** out_recipe);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_setup_input_recipe(elite_rtsi_client_handle_t* handle, const char* recipe_csv,
                                                                     elite_rtsi_recipe_handle_t** out_recipe);
ELITE_C_EXPORT void elite_rtsi_recipe_destroy(elite_rtsi_recipe_handle_t* recipe);

ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_start(elite_rtsi_client_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_pause(elite_rtsi_client_handle_t* handle, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_send(elite_rtsi_client_handle_t* handle, elite_rtsi_recipe_handle_t* recipe);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_receive_data(elite_rtsi_client_handle_t* handle,
                                                               elite_rtsi_recipe_handle_t* recipe, int32_t read_newest,
                                                               int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_receive_data_multi(elite_rtsi_client_handle_t* handle,
                                                                     elite_rtsi_recipe_handle_t** recipes, int32_t recipe_count,
                                                                     int32_t read_newest, int32_t* out_received_index);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_is_connected(elite_rtsi_client_handle_t* handle, int32_t* out_connected);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_is_started(elite_rtsi_client_handle_t* handle, int32_t* out_started);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_client_is_read_available(elite_rtsi_client_handle_t* handle,
                                                                    int32_t* out_available);

ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_get_id(elite_rtsi_recipe_handle_t* recipe, int32_t* out_id);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_get_recipe_csv(elite_rtsi_recipe_handle_t* recipe, char* out_buffer,
                                                                 int32_t buffer_len, int32_t* out_required_len);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_get_value_double(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                   double* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_get_value_int32(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                  int32_t* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_get_value_uint32(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                   uint32_t* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_get_value_bool(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                 int32_t* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_get_value_vector6d(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                     double* out_value6, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_set_value_double(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                   double value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_set_value_int32(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                  int32_t value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_set_value_uint32(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                   uint32_t value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_set_value_bool(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                 int32_t value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_recipe_set_value_vector6d(elite_rtsi_recipe_handle_t* recipe, const char* name,
                                                                     const double* value6, int32_t* out_success);

ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_create(const char* output_recipe_csv, const char* input_recipe_csv,
                                                     double frequency, elite_rtsi_io_handle_t** out_handle);
ELITE_C_EXPORT void elite_rtsi_io_destroy(elite_rtsi_io_handle_t* handle);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_connect(elite_rtsi_io_handle_t* handle, const char* ip, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_disconnect(elite_rtsi_io_handle_t* handle);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_is_connected(elite_rtsi_io_handle_t* handle, int32_t* out_connected);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_is_started(elite_rtsi_io_handle_t* handle, int32_t* out_started);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_controller_version(elite_rtsi_io_handle_t* handle,
                                                                     elite_version_info_t* out_version);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_speed_scaling(elite_rtsi_io_handle_t* handle, double scaling,
                                                                int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_standard_digital(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                   int32_t level, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_configure_digital(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                    int32_t level, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_analog_output_voltage(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                        double value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_analog_output_current(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                        double value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_external_force_torque(elite_rtsi_io_handle_t* handle,
                                                                        const double* value6, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_tool_digital_output(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                      int32_t level, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_timestamp(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_payload_mass(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_payload_cog(elite_rtsi_io_handle_t* handle, double* out_value3);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_script_control_line(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_target_joint_positions(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_target_joint_velocity(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_joint_positions(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_joint_torques(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_joint_velocity(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_joint_current(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_joint_temperatures(elite_rtsi_io_handle_t* handle,
                                                                            double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_tcp_pose(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_tcp_velocity(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_tcp_force(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_target_tcp_pose(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_target_tcp_velocity(elite_rtsi_io_handle_t* handle, double* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_digital_input_bits(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_digital_output_bits(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_robot_mode(elite_rtsi_io_handle_t* handle, int32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_joint_mode(elite_rtsi_io_handle_t* handle, int32_t* out_value6);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_safety_status(elite_rtsi_io_handle_t* handle, int32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_robot_status(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_runtime_state(elite_rtsi_io_handle_t* handle, int32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_actual_speed_scaling(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_target_speed_scaling(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_robot_voltage(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_robot_current(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_elbow_position(elite_rtsi_io_handle_t* handle, double* out_value3);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_elbow_velocity(elite_rtsi_io_handle_t* handle, double* out_value3);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_safety_status_bits(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_analog_io_types(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_analog_input(elite_rtsi_io_handle_t* handle, int32_t index,
                                                               double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_analog_output(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_io_current(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_mode(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_analog_input_type(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_analog_output_type(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_analog_input(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_analog_output(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_output_voltage(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_output_current(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_output_temperature(elite_rtsi_io_handle_t* handle, double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_digital_mode(elite_rtsi_io_handle_t* handle, uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_tool_digital_output_mode(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                           uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_out_bool_registers0_to_31(elite_rtsi_io_handle_t* handle,
                                                                             uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_out_bool_registers32_to_63(elite_rtsi_io_handle_t* handle,
                                                                              uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_in_bool_registers0_to_31(elite_rtsi_io_handle_t* handle,
                                                                            uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_in_bool_registers32_to_63(elite_rtsi_io_handle_t* handle,
                                                                             uint32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_in_bool_register(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                   int32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_out_bool_register(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                    int32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_in_int_register(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                  int32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_out_int_register(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                   int32_t* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_in_double_register(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                     double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_out_double_register(elite_rtsi_io_handle_t* handle, int32_t index,
                                                                      double* out_value);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_recipe_value_double(elite_rtsi_io_handle_t* handle, const char* name,
                                                                      double* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_recipe_value_int32(elite_rtsi_io_handle_t* handle, const char* name,
                                                                     int32_t* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_recipe_value_uint32(elite_rtsi_io_handle_t* handle, const char* name,
                                                                      uint32_t* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_recipe_value_bool(elite_rtsi_io_handle_t* handle, const char* name,
                                                                    int32_t* out_value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_recipe_value_vector3d(elite_rtsi_io_handle_t* handle, const char* name,
                                                                        double* out_value3, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_get_recipe_value_vector6d(elite_rtsi_io_handle_t* handle, const char* name,
                                                                        double* out_value6, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_input_recipe_value_double(elite_rtsi_io_handle_t* handle, const char* name,
                                                                            double value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_input_recipe_value_int32(elite_rtsi_io_handle_t* handle, const char* name,
                                                                           int32_t value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_input_recipe_value_uint32(elite_rtsi_io_handle_t* handle, const char* name,
                                                                            uint32_t value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_input_recipe_value_bool(elite_rtsi_io_handle_t* handle, const char* name,
                                                                          int32_t value, int32_t* out_success);
ELITE_C_EXPORT elite_c_status_t elite_rtsi_io_set_input_recipe_value_vector6d(elite_rtsi_io_handle_t* handle, const char* name,
                                                                               const double* value6, int32_t* out_success);

ELITE_C_EXPORT const char* elite_rtsi_client_last_error_message(elite_rtsi_client_handle_t* handle);
ELITE_C_EXPORT const char* elite_rtsi_io_last_error_message(elite_rtsi_io_handle_t* handle);

#ifdef __cplusplus
}
#endif

#endif
