#include "Saturn/Defines.h"

#include <spdlog/spdlog.h>
#include <spdlog/fmt/ostr.h>

class Log {
public:
    static void Init();

    inline static TSharedPtr<spdlog::logger>& GetCoreLogger() { return s_CoreLogger; }
private:
    static TSharedPtr<spdlog::logger> s_CoreLogger;
};

// Core logging macros
#define LOG_CRITICAL(...)   ::Log::GetCoreLogger()->critical(__VA_ARGS__)
#define LOG_ERROR(...)      ::Log::GetCoreLogger()->error(__VA_ARGS__)
#define LOG_WARN(...)       ::Log::GetCoreLogger()->warn(__VA_ARGS__)
#define LOG_INFO(...)       ::Log::GetCoreLogger()->info(__VA_ARGS__)
#define LOG_TRACE(...)      ::Log::GetCoreLogger()->trace(__VA_ARGS__)