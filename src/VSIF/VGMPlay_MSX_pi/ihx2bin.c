/* ----------------------------------------------------- */
/*	IntelHEX file to BSAVE converter					 */
/* ===================================================== */
/*	2009/09/05	t.hara									 */
/* ----------------------------------------------------- */

#include <stdio.h>
#include <string.h>
#include <ctype.h>

/* ----------------------------------------------------- */
static int get_hex2( const unsigned char *p_hex ) {
	int digit;

	if( !isxdigit(p_hex[0]) || !isxdigit(p_hex[1]) ) {
		fprintf( stderr, "WARNNING: 不正な文字があります\n" );
		return -1;
	}
	if( isdigit(p_hex[0]) ) {
		digit = p_hex[0] - '0';
	}
	else {
		digit = toupper(p_hex[0]) - 'A' + 10;
	}
	digit <<= 4;
	if( isdigit(p_hex[1]) ) {
		digit |= p_hex[1] - '0';
	}
	else {
		digit |= toupper(p_hex[1]) - 'A' + 10;
	}
	return digit;
}

/* ----------------------------------------------------- */
static void convert_hex_to_bin( unsigned char *p_memory, int address, int data_length, char *p_buffer ) {
	int count;

	for( count = 0; count < data_length; count++ ) {
		p_memory[ address + count ] = get_hex2( p_buffer + count * 2 );
	}
}

/* ----------------------------------------------------- */
static int load_hex_file( unsigned char *p_memory, const char *p_name, int *p_start_address, int *p_end_address ) {
	FILE *p_file;
	char s_buffer[ 1024 ];
	int data_length, address, record_type;

	*p_start_address	= 0x0FFFF;
	*p_end_address		= 0x00000;

	p_file = fopen( p_name, "rb" );
	if( p_file == NULL ) {
		return 0;
	}
	while( !feof(p_file) ) {
		memset( s_buffer, 0, sizeof(s_buffer) );
		if( fgets( s_buffer, sizeof(s_buffer), p_file ) == NULL ) {
			break;
		}
		if( s_buffer[0] != ':' ) {
			continue;
		}
		data_length	= get_hex2( s_buffer + 1 );
		address		=(get_hex2( s_buffer + 3 ) << 8) | get_hex2( s_buffer + 5 ); 
		record_type	= get_hex2( s_buffer + 7 );
		switch( record_type ) {
		case 0x00:	/* データタイプ */
		case 0x01:	/* エンドレコード */
			break;
		case 0x02:	/* 拡張レコードタイプ */
		case 0x03:	/* スタートセグメントレコード */
		case 0x04:	/* 拡張リニアアドレスレコード */
		case 0x05:	/* スタートリニアアドレス */
		default:	/* その他 */
			fprintf( stderr, "WARNNING: 非対応のレコードタイプ(0x%02X)が見つかりました。この行は無視します。\n" );
			continue;
		}
		if( record_type == 1 ) {
			break;
		}
		if( address < *p_start_address ) {
			*p_start_address = address;
		}
		if( (address + data_length - 1) > *p_end_address ) {
			*p_end_address = address + data_length - 1;
		}
		convert_hex_to_bin( p_memory, address, data_length, s_buffer + 9 );
	}
	fclose( p_file );
	return 1;
}

/* ----------------------------------------------------- */
static int msx_bsave( const unsigned char *p_memory, int start_address, int end_address, const char *p_name ) {
	FILE *p_file;
	char header[7];

	start_address &= 0x0FFFF;
	end_address &= 0x0FFFF;

	if( end_address < start_address ) {
		return 0;
	}
	header[0] = 0xFE;
	header[1] = start_address & 255;
	header[2] = (start_address >> 8) & 255;
	header[3] = end_address & 255;
	header[4] = (end_address >> 8) & 255;
	header[5] = start_address & 255;
	header[6] = (start_address >> 8) & 255;

	p_file = fopen( p_name, "wb" );
	if( p_file == NULL ) {
		return 0;
	}
	fwrite( header, 7, 1, p_file );
	fwrite( p_memory + start_address, end_address - start_address + 1, 1, p_file );
	fclose( p_file );
	return 1;
}
 
/* ----------------------------------------------------- */
static void usage( const char *p_name ) {

	fprintf( stderr, "Usage> %s <in.ihx> <out.bin> [<start> [<end>]]\n", p_name );
}

/* ----------------------------------------------------- */
int main( int argc, char *argv[] ) {
	unsigned char target_memory[ 65536 ];
	int start_address = 0;
	int end_address = 0;

	printf( "IHX2BIN\n" );
	printf( "===========================================\n" );
	printf( "2009/09/05 t.hara\n" );

	if( argc < 3 ) {
		usage( argv[0] );
		return 1;
	}
	if( !load_hex_file( target_memory, argv[1], &start_address, &end_address ) ) {
		fprintf( stderr, "ERROR: %s を正常に読み込めませんでした\n", argv[1] );
		return 2;
	}
	if( argc >= 4 ) {
		sscanf( argv[3], "%i", &start_address );
	}
	if( argc >= 5 ) {
		sscanf( argv[4], "%i", &end_address );
	}
	if( !msx_bsave( target_memory, start_address, end_address, argv[2] ) ) {
		fprintf( stderr, "ERROR: %s を正常に書き込めませんでした\n", argv[2] );
		return 3;
	}
	printf( "Complete.\n" );
	return 0;
}
