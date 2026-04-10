// SPDX-License-Identifier: MIT
#ifndef __ELITE_SERIALCOMMUNICATION_INTERNAL_HPP__
#define __ELITE_SERIALCOMMUNICATION_INTERNAL_HPP__

#include <Elite/SerialCommunication.hpp>

struct elite_serial_comm_handle_t {
    ELITE::SerialCommunicationSharedPtr comm;
};

#endif
