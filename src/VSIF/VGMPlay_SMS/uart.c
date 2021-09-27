#include "uart.h"

#include <stdint.h>

uint8_t uart_result;
uint8_t uart_status;

uint8_t uart_get_status() { return uart_status; }
