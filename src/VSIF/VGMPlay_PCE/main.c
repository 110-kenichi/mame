
#include "main.h"

// void romPath();
// void romRead();
// void fileRead();
// void fileRead();
// void fileWrite();
// void fileToRom();
// void folderList();
// void sysTimer();
// void usbWrite();
void usbRead();

//static u8 rom_path[512];

//https://nicotakuya.hatenablog.com/entry/2020/12/04/010457

// const char waveform[]={
//  18,22,24,26,28,28,30,30,30,30,28,28,26,24,22,18,
//  12, 8, 6, 4, 2, 2, 0, 0, 0, 0, 2, 2, 4, 6, 8,12
// };

// #define FREQ12BIT 254

// /*12bit freq = (3580000/32)/Frequency */

// char *psgch;    /*Channel*/
// char *psgmvol;  /*Master Volume*/
// char *psgfreql; /*Frequency*/
// char *psgfreqh; /*Frequency*/
// char *psgctrl;  /*control*/
// char *psglrvol; /*L/R Volume*/
// char *psgdata;  /*Waveform*/

void main() {
	int i;

    sysInit();
    bi_init();

    gConsPrintCX("MAMI VGM SOUND DRIVER FOR ED BY ITOKEN");
    //gConsPrintCX("EverDrive IO sample");
    //gSetXY(0, 2);

    // romPath(); //get current rom path
    // romRead(); //read rom memory
    // fileRead(); //read from file
    // fileWrite(); //write to the file
    // fileToRom(); //fast dma transfer from file to the rom memory
    // folderList(); //print list of files in the folder
    // sysTimer(); //system ms timer

    // gConsPrint("");
    // gConsPrint("press any key when ready to test usb");
    // while (sysJoyRead() == 0);
    // gCleanScreen();

    //usbWrite(); //tx via usb to pc

	// psgch   = 0x800;
	// psgmvol = 0x801;
	// psgfreql= 0x802;
	// psgfreqh= 0x803;
	// psgctrl = 0x804;
	// psglrvol= 0x805;
	// psgdata = 0x806;

	// *psgch = 0;		/*Channel*/

	// *psgctrl = (1<<6); 	/*reset*/
	// *psgctrl = (0<<6); 	/*waveform transfer*/
	// for(i=0;i<32;i++){
	// 	*psgdata = waveform[i];	/*Waveform*/
	// }

	// *psgfreql = FREQ12BIT & 0xff;	/*Frequency L*/
	// *psgfreqh = FREQ12BIT >> 8;	/*Frequency H*/

	// *psglrvol = 0xff;	/*L/R Volume*/
	// *psgmvol = 0xee;	/*Master Volume*/
	// *psgctrl = (1<<7)+ 0x1f; /*Channel on*/

    usbRead(); //rx from via usb from pc

    while (1);
}

// void romPath() {

//     bi_cmd_rom_path(rom_path, 0); //buffer size should be not less than 512B

//     gConsPrint("rom path : ");
//     gAppendString_ML(rom_path, 27);

// }

// void romRead() {

//     u8 buff[12];

//     gConsPrint("rom read : ");
//     bi_mem_rdd(16, buff, sizeof (buff)); //read rom memory at offset 16
//     gAppendHex(buff, sizeof (buff));
// }

// void fileRead() {

//     u8 resp;
//     u8 buff[12];
//     u32 size;

//     resp = bi_cmd_file_open("edturbo/menu.dat", FA_READ);
//     if (resp)return; //error
//     size = bi_cmd_file_available();
//     resp = bi_cmd_file_read(buff, sizeof (buff));
//     if (resp)return; //error
//     resp = bi_cmd_file_close();
//     if (resp)return; //error

//     gConsPrint("file read: ");
//     gAppendHex(buff, sizeof (buff));
//     gConsPrint("file size: 0x");
//     gAppendHex32(size);
// }

// void fileWrite() {

//     u8 resp;
//     u8 buff[] = {'h', 'e', 'l', 'l', 'o', ' ', 't', 'e', 'x', 't'};

//     gConsPrint("file write...");
//     //create "test_file.txt" in the root of SD and write buff to the file.
//     resp = bi_cmd_file_open("test_file.txt", FA_WRITE | FA_CREATE_ALWAYS);
//     if (resp)return; //error
//     resp = bi_cmd_file_write(buff, sizeof (buff));
//     if (resp)return; //error
//     resp = bi_cmd_file_close();
//     if (resp)return; //error
//     gAppendString("ok");

// }

// void fileToRom() {

//     u8 resp;
//     u32 size;

//     gConsPrint("read file to rom...");

//     resp = bi_cmd_file_open("edturbo/menu.dat", FA_READ);
//     if (resp)return; //error
//     size = bi_cmd_file_available();
//     resp = bi_cmd_file_read_mem(0x20000, size); //write whole file to the rom memory at offset 0x20000
//     if (resp)return; //error
//     resp = bi_cmd_file_close();
//     if (resp)return; //error

//     gAppendString("ok");
// }

// void folderList() {

//     u8 resp;
//     u16 size;
//     u8 i;
//     FileInfo inf;
//     u8 name_buff[16 + 1];

//     inf.file_name = name_buff;

//     gConsPrint("");
//     gConsPrint("get folder list...");

//     resp = bi_cmd_dir_load("edturbo", 0); //get list of files in system folder
//     if (resp)return; //error
//     bi_cmd_dir_get_size(&size);

//     gAppendString("dir size: ");
//     gAppendNum(size);

//     bi_cmd_dir_get_recs(0, size, sizeof (name_buff) - 1);

//     for (i = 0; i < size && i < 8; i++) {

//         resp = bi_rx_next_rec(&inf);
//         if (resp)return; //error
//         gConsPrint("[");
//         gAppendString(inf.file_name);
//         gAppendString("]");

//         if (inf.is_dir) {
//             gAppendString("...dir");
//         } else {
//             gAppendString("...file");
//         }
//     }

//     gConsPrint("");
// }

// void sysTimer() {

//     u8 i;
//     u16 time;
//     gConsPrint("one frame time: ");

//     gVsync(); //vsync alignment
//     time = bi_get_ticks();
//     for (i = 0; i < 10; i++) {//10 iteration for accurate measurement
//         gVsync();
//     }
//     time = bi_get_ticks() - time;
//     time /= 10;

//     gAppendNum(time);
//     gAppendString("ms");

// }

// void usbWrite() {//send strings to the virtual com-port. Use any serial terminal app to receive strings.

//     gConsPrint("send test string to usb...");
//     bi_cmd_usb_wr("test string\n", 12);

// }

u8 snd_data[3];
u8 *snd_reg;

void usbRead() {
    // gConsPrint("waiting for input string from usb...");
    // gConsPrint("");

    while (1) {
        //single byte communication is very slow. 
        //use larger blocks for real applications but not more than SIZE_FIFO
        bi_fifo_rd(snd_data, 3);
        switch(*snd_data)
        {
            case 0:
            {
                snd_reg = 0x800 | *(snd_data+1);
                *snd_reg = *(snd_data+2);
                break;
            }
        }
        // if (char_val == '\n') {
        //     gConsPrint("");
        // } else {
        //     gAppendChar(char_val);
        // }
    }


}


