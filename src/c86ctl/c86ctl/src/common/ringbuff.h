/***
	RingBuffer module
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com

	note:
	Lock-Free タイプのリングバッファルーチン。
	read１スレッド、write１スレッドまではLockせずに使えます。
	ただし、read, writeが２スレッド以上同時にアクセスされる場合は
	ぶっ壊れるので外部保護してください。
 */

#ifndef RINGBUFF_H_
#define RINGBUFF_H_


template <class T>
class CRingBuff {
protected:
	T *p;
	UINT sz;
	UINT mask;
	volatile UINT widx;
	volatile UINT ridx;
	
public:
	CRingBuff(){
		p = NULL;
		sz = 0;
		mask = 0;
		widx = 0;
		ridx = 0;
	};

	~CRingBuff(){
		freeres();
	};
	
	BOOL alloc( UINT asize ){
		freeres();
		p = new T[asize];
		if( p ){
			sz = asize;
			mask = sz - 1;
			widx = 0;
			ridx = 0;
		}
		return p ? TRUE : FALSE;
	};

	VOID freeres(VOID){
		if( p )
			delete [] p;

		p = NULL;
		sz = 0;
		mask = 0;
		widx = 0;
		ridx = 0;
	};

	// 残りバッファ量を返す
	UINT remain(VOID){
		if( !p ) return 0;
		UINT cridx = ridx & mask;
		UINT cwidx = widx & mask;
		if( cridx <= cwidx ){
			return cridx + (sz - cwidx) - 1;
		}else{
			return cridx - cwidx - 1;
		}
	};

	// 格納済みデータ量を返す
	UINT length(VOID){
		if( !p ) return 0;
		UINT cridx = ridx & mask;
		UINT cwidx = widx & mask;
		if( cridx <= cwidx ){
			return cwidx - cridx;
		}else{
			return cwidx + (sz - cridx);
		}
	};

	// 格納されているデータが無いかどうか
	BOOL isempty(void){
		if( !p ) return 0;
		return ( length() == 0 );
	};

	void flush(void){
		while( 0<length() )
			::InterlockedIncrement(&ridx);
	};

	T* front(void){
		if( !p )
			return 0;
		
		return p + (ridx&mask);
	};
	
	BOOL pop( T *data ){
		if( !p )
			return FALSE;

		*data = *(p + (ridx&mask));
		::InterlockedIncrement(&ridx);
		return TRUE;
	};

	BOOL pop( T *data, UINT sz ){
		if( !p ) return FALSE;
		if( length() < sz ) return FALSE;

		for( UINT i=0; i<sz; i++ ){
			T *pd = p + (ridx&mask);
			data[i] = *pd;
			::InterlockedIncrement(&ridx);
		}
		return TRUE;
	};

	VOID push( T data ){
		if( !p ) return;
		while( remain() < 1 ){
			Sleep(1);
		}

		T *pd = p + (widx&mask);
		*pd = data;
		::InterlockedIncrement(&widx);
	};

	BOOL push( const T *data, UINT sz ){

		if( !p ) return FALSE;
		while( remain() < sz ){
			Sleep(1);
		}

		for( UINT i=0; i<sz; i++ ){
			T *pd = p + (widx&mask);
			*pd = *data++;
			::InterlockedIncrement(&widx);
		}
		return TRUE;
	};
	

};

#endif

