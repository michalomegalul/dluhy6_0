#include <iostream>
#include <chrono>
#include <stdlib.h>

void allocbs() {
  const size_t size = 1024*1024*1024;
  void* memory = malloc(size*sizeof(int));
  for(uint64_t i = 0; i < size; i++) {
    reinterpret_cast<int*>(memory)[i] = 0xdeadbeef;
  };
}

int main() {
  for(uint32_t i = 0; i < 100; i++) {
    allocbs();
  }
  while(true);
};