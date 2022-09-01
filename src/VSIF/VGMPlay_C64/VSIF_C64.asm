*=$0801
BasicUpstart(main)

.zp
{
    addr:
        .byte 0
    data:
        .byte 0
    data2:
        .byte 0
}

* = $0810

//CLOCK_PAL  =  985248 Hz
//CLOCK_NTSC = 1022727 Hz

//  31250bps 31.527936 clk @ 985248 (PAL)
//  31250bps 32.727264 clk @ 1022727 Hz (NTSC)

.macro CheckStartBit1()
{
    loop:
        lda $dc00            //3  3 //Read port2 data
        and #%00001000       //2  5 // Fire Bit is 0? 
        beq loop             //2  7
        lda $dc00            //3 10 //Read port2 data
}

.macro CheckStartBit0()
{
    loop:
        lda $dc00            //3  3 //Read port2 data
        and #%00001000       //2  5 // Fire Bit is 0? 
        bne loop             //2  7
        lda $dc00            //3 10 //Read port2 data
}

main:
    jsr $e544 //clear screen

    //Next we tell the VIC that we want to enter single-color text mode. Inserting 1b into $d011 means “Enter text-mode”, and inserting 08 into $d016 means “Use single-color”. We also tell the VIC that our screen RAM is at $0400 and that we want to use the default charset by inserting 14 into $d018 (see earlier tutorials for more information about how this works).
    lda #$1b
    ldx #$08
    ldy #$14
    sta $d011
    stx $d016
    sty $d018

    ldx #<text1        //string address least significant byte (LSB)
    ldy #>text1        //string address most significant byte (MSB)
    jsr sprint

    sei //disable int
    //Then we put the value 7f into $dc0d and $dd0d to disable the CIA I, CIA II and VIC interrupts (timer, keyboard,…, interrupts)
    lda #$7f
    sta $dc0d
    sta $dd0d

    //https://sta.c64.org/cbm64mem.html
    lda #$e0        //disable keyb
    sta $dc02

    //lda %01000000   //Select Paddle #1
    lda #%10000000      //Select Paddle #2
    sta $dc00           //Bits #6-#7: Paddle selection; %01 = Paddle #1; %10 = Paddle #2.

/* ------------------------------------------------------------------ */
//http://codebase64.org/doku.php?id=base:joystick_input_handling    
// Bit #0: 0 = Port 2 joystick up pressed.
// Bit #1: 0 = Port 2 joystick down pressed.
// Bit #2: 0 = Port 2 joystick left pressed.
// Bit #3: 0 = Port 2 joystick right pressed.
// Bit #4: 0 = Port 2 joystick fire pressed.
wait_start_bit:
    lda $dc00            //3  3 //Read port2 data
    and #%00011000       //2  5 // Fire Bit is 0? 
    bne wait_start_bit   //2  7
get_address_Lo:
    lda $dc00            //3 10 //Read port2 data
    and #%00000111       //2 12
    sta addr             //4 16

// adrs_lo, mid, hi, data_hi, mid, lo, ...
/* ------------------------------------------------------------------ */
get_address_Mid:
    CheckStartBit1()     //10 10
    and #%00000111       //2 12
    asl                  //2 14
    asl                  //2 16
    asl                  //2 18
    ora addr             //4 22
    sta addr             //4 26

get_address_Hi:
    CheckStartBit0()     //10 10
    and #%00000011      //2 12
    ror                 //2 14
    ror                 //2 16
    ror                 //2 18
    ora addr            //4 24
    tax                 //2 26

get_data_Hi:
    CheckStartBit1()     //10 10
    and #%00000111     //2 12
    ror                //2 14
    ror                //2 16
    ror                //2 18
    ror                //2 20
    sta data           //4 24

get_data_Mid:
    CheckStartBit0()     //10 10
    and #%00000111     //2 12
    asl                //2 14
    asl                //2 16
    ora data           //4 20
    sta data           //4 24

get_data_Lo:
    CheckStartBit1()     //10 10
    and #%0000_0100      //2 12
    bne wait_next_data2  //2 14

    lda $dc00           //3 17 //Read port2 data
    and #%00000011      //2 19
    //Sound SID
    ora data            //4 23
    sta $d400,x         //6 29
    jmp wait_start_bit  //2 31


wait_next_data2:
    lda $dc00           //3 17 //Read port2 data
    and #%00000011      //2 19
    ora data            //4 23
    sta data            //4 27

get_data_Hi2:
    CheckStartBit0()     //10 10
    and #%00000111     //2 12
    ror                //2 14
    ror                //2 16
    ror                //2 18
    ror                //2 20
    sta data2           //4 24

get_data_Mid2:
    CheckStartBit1()     //10 10
    and #%00000111     //2 12
    asl                //2 14
    asl                //2 16
    ora data2           //4 20
    sta data2           //4 24

get_data_Lo2:
    CheckStartBit0()    //10 10
    and #%00000011      //2 12
    //Sound SID
    ora data2           //4 16
    sta $d400,x         //6 22
    inx                 //2 24
    lda data            //3 27
    sta $d400,x         //6 33

    jmp wait_start_bit  //3 36

//     lda $dc00          //3 25 //Read port2 data
//     and #%0000_0100    //2 27
//     beq wait_start_bit_jmp  //2 29
//     inx                //2 31
//     jmp get_data_Hi    //3 33
// wait_start_bit_jmp:
//     jmp wait_start_bit //3 32
/* ------------------------------------------------------------------ */

/* ------------------------------------------------------------------ */

//https://www.c64-wiki.com/wiki/Assembler_Example
//bsout    =$ffd2                //kernel character output sub
//ptr      =$fb                  //zero page pointer
//
sprint:
    stx $fb               //save string pointer LSB
    sty $fb+1             //save string pointer MSB
    ldy #0                //starting string index
//
sprint01:
    lda ($fb),y           //get a character
    beq sprint02          //end of string
//
    jsr $ffd2             //print character
    iny                   //next
    bne sprint01
//
sprint02:
    rts                   //exit

    .encoding "screencode_mixed"
text1:
    .text "READY VSIF"
    .byte 0
