
// c86winDlg.h : ヘッダー ファイル
//

#pragma once
#include "CS98Data.h"
#include "afxwin.h"

// C86winDlg ダイアログ
class C86winDlg : public CDialogEx
{
// コンストラクション
public:
	C86winDlg(CWnd* pParent = NULL);	// 標準コンストラクター

// ダイアログ データ
	enum { IDD = IDD_C86WIN_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV サポート


// 実装
protected:
	CS98Data s98data;

protected:
	HICON m_hIcon;

	// 生成された、メッセージ割り当て関数
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedCancel();
	afx_msg void OnBnClickedButtonPlay();
	afx_msg void OnBnClickedButtonStop();
	afx_msg void OnBnClickedButtonOpen();

public:
	static unsigned int WINAPI PlayerThread(LPVOID param);
	static bool terminateFlag;
	UINT threadID;
	HANDLE hThread;

	UINT m_tick;

	CEdit m_editFilePath;
	CStatic m_staticTick;
	CEdit m_editMessage;
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedButtonInitialize();
	afx_msg void OnBnClickedButtonDeinitialize();
	afx_msg void OnBnClickedButtonSetSSGVol();
	afx_msg void OnBnClickedButtonSetPllClock();
	UINT m_ssgVol;
	UINT m_pllClock;
	afx_msg void OnBnClickedButtonMbinfo();
	afx_msg void OnBnClickedButtonModuleinfo();
	afx_msg void OnBnClickedButtonGetSsgvol();
	afx_msg void OnBnClickedButtonGetPllclock();
	afx_msg void OnDestroy();
	afx_msg void OnBnClickedButtonGetFwver();
	afx_msg void OnBnClickedButtonTest1();
	afx_msg void OnBnClickedButtonAdpcmZeroreset();
	afx_msg void OnBnClickedButtonSwreset();
};
