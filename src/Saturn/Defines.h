#pragma once

#include <vector>
#include <memory>
#include <intrin.h>
#include <cstdint>
#include <string>
#include <type_traits>
#include <parallel_hashmap/phmap.h>

template <typename K, typename V>
using TMap = phmap::parallel_node_hash_map<K, V>;

typedef uint32_t FNameEntryId;

template <typename T>
using TUniquePtr = std::unique_ptr<T>;

template <typename T>
using TSharedPtr = std::shared_ptr<T>;

template <typename T>
using TWeakPtr = std::weak_ptr<T>;

#define ENUM_CLASS_FLAGS(Enum) \
	export inline           Enum& operator|=(Enum& Lhs, Enum Rhs) { return Lhs = (Enum)((__underlying_type(Enum))Lhs | (__underlying_type(Enum))Rhs); } \
	export inline           Enum& operator&=(Enum& Lhs, Enum Rhs) { return Lhs = (Enum)((__underlying_type(Enum))Lhs & (__underlying_type(Enum))Rhs); } \
	export inline           Enum& operator^=(Enum& Lhs, Enum Rhs) { return Lhs = (Enum)((__underlying_type(Enum))Lhs ^ (__underlying_type(Enum))Rhs); } \
	export inline constexpr Enum  operator| (Enum  Lhs, Enum Rhs) { return (Enum)((__underlying_type(Enum))Lhs | (__underlying_type(Enum))Rhs); } \
	export inline constexpr Enum  operator& (Enum  Lhs, Enum Rhs) { return (Enum)((__underlying_type(Enum))Lhs & (__underlying_type(Enum))Rhs); } \
	export inline constexpr Enum  operator^ (Enum  Lhs, Enum Rhs) { return (Enum)((__underlying_type(Enum))Lhs ^ (__underlying_type(Enum))Rhs); } \
	export inline constexpr bool  operator! (Enum  E)             { return !(__underlying_type(Enum))E; } \
	export inline constexpr Enum  operator~ (Enum  E)             { return (Enum)~(__underlying_type(Enum))E; }


template<typename Enum>
constexpr bool EnumHasAnyFlags(Enum Flags, Enum Contains) {
	return (((__underlying_type(Enum))Flags) & (__underlying_type(Enum))Contains) != 0;
}

template<int32_t Size, uint32_t Alignment>
struct TAlignedBytes {
	alignas(Alignment) uint8_t Pad[Size];
};

template<typename NumType, typename = typename std::enable_if<std::is_arithmetic<NumType>::value, NumType>::type>
NumType Align(NumType ptr, int alignment) {
	return ptr + alignment - 1 & ~(alignment - 1);
}