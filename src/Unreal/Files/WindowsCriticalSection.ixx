module;

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

export module Saturn.Files.WindowsCriticalSection;

/**
 * This is the Windows version of a critical section. It uses an aggregate
 * CRITICAL_SECTION to implement its locking.
 */
export class FWindowsCriticalSection {
public:
    FWindowsCriticalSection(const FWindowsCriticalSection&) = delete;
    FWindowsCriticalSection& operator=(const FWindowsCriticalSection&) = delete;

    /**
     * Constructor that initializes the aggregated critical section
     */
    FORCEINLINE FWindowsCriticalSection() {
        InitializeCriticalSection(&CriticalSection);
        SetCriticalSectionSpinCount(&CriticalSection, 4000);
    }

    /**
     * Destructor thata deletes the critical section
     */
    FORCEINLINE ~FWindowsCriticalSection() {
        DeleteCriticalSection(&CriticalSection);
    }

    /**
     * Locks the critical secion
     */
    FORCEINLINE void Lock() {
        EnterCriticalSection(&CriticalSection);
    }

    /**
     * Attempt to take a lock and returns whether or not a lock was taken.
     *
     * @return true if a lock was takenn, false otherwise.
     */
    FORCEINLINE bool TryLock() {
        if (TryEnterCriticalSection(&CriticalSection)) {
            return true;
        }
        return false;
    }

    /**
     * Releases the lock on the critical section
     * 
     * Calling this when not locked in undefined behavior & may cause indefinite waiting on next lock.
     * See: https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-leavecriticalsection#remarks
     */
    FORCEINLINE void Unlock() {
        LeaveCriticalSection(&CriticalSection);
    }
private:
    // The underlying critical section object.
    CRITICAL_SECTION CriticalSection;
};

export typedef FWindowsCriticalSection FCriticalSection;