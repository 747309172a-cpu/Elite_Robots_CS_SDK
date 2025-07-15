#ifndef __RT_UTILS_HPP__
#define __RT_UTILS_HPP__

#include <thread>
#include <vector>


namespace ELITE
{

namespace RT_UTILS
{

/**
 * @brief Set the thread to FIFO scheduling and set the priority.
 * 
 * @param thread Thread handle
 * @param priority Thread priority
 * @return true success
 * @return false fail
 */
bool setThreadFiFoScheduling(std::thread::native_handle_type& thread, const int priority);

/**
 * @brief Get the maximum priority of the thread.
 * 
 * @return int The max thread priority
 */
int getThreadFiFoMaxPriority();

/**
 * @brief Bind a thread to a specific CPU core to run.
 * 
 * @param thread Thread handle
 * @param cpu CPU core index
 * @return true success
 * @return false fail
 */
bool bindThreadToCpus(std::thread::native_handle_type& thread, const int cpu);

} // namespace RT_UTILS


} // namespace ELITE


#endif
