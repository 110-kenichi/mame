#ifndef __UART_H
#define __UART_H

#include <stdint.h>

/**Stores the result of an uart_getc operation.*/
extern uint8_t uart_result;
/**Stores the result status of an uart_getc operation.*/
extern uint8_t uart_status;

#define UART_STATUS_OK  0x00
#define UART_STATUS_NOK 0xFF

void    uart_processVgm();

void    uart_getc();

/**Samples serial line and receives a byte.
 *
 * Sampled byte is stored on uart_result and uart_status stores the success
 * or not of the operation. A UART_STATUS_OK on the status means a byte has
 * been detected and has been correctly sampled, UART_STATUS_NOK usually
 * means a framing error has happened.
 */
void    uart_getc2();
void    uart_putc2(uint8_t c);

void    uart_retMamiId();
void    uart_retTrue();
void    uart_retFalse();

/**Returns uart operation status.
 *
 * @returns Value of uart_status.
 */
uint8_t uart_get_status();

#endif
