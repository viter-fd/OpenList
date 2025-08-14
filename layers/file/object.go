package file

import "time"

// HostFileObject 驱动层获取获取的文件信息
type HostFileObject struct {
	realName []string  // 真实名称
	previews []string  // 文件预览
	fileSize int64     // 文件大小
	lastTime time.Time // 修改时间
	makeTime time.Time // 创建时间
	fileType bool      // 文件类型
	fileHash string    // 文件哈希
	hashType int16     // 哈希类型
}

// UserFileObject 由用户层转换后的文件信息
type UserFileObject struct {
	HostFileObject
	showPath []string // 文件路径
	showName []string // 文件名称
	realPath []string // 真实路径
	checksum int32    // 密码校验
	fileMask int16    // 文件权限
	encrypts int16    // 文件状态

	// 下列信息用于前端展示文件用
	enc_type string // 加解密类型
	enc_from string // 文件密码源
	enc_pass string // 加解密密码
	com_type string // 压缩的类型
	sub_nums int16  // 子文件数量

	// 下列信息用于后端内部处理用
	// fileMask =================
	// 占用：000000 0 000 000 000
	// 含义：ABCDEF 1 421 421 421
	// A-加密 B-前端解密 C-自解密
	// D-is分卷 E-is压缩 F-is隐藏
	// encrypts =================
	// 占用位：0000000000 00 0000
	// 含义为：分卷数量 压缩 加密
}

type PermissionFile struct {
}

type LinkFileObject struct {
	download []string // 下载链接
	usrAgent []string // 用户代理
}

type ListFileOption struct {
}

type FindFileOption struct {
}

type KillFileOption struct {
}
type MakeFileOption struct {
}
type DownloadOption struct {
	downType int8 // 下载类型

}
type UploaderOption struct {
}
type BackFileAction struct {
	success bool   // 是否成功
	message string // 错误信息
}
type NewShareAction struct {
	BackFileAction
	shareID string    // 分享编码
	pubUrls string    // 公开链接
	passkey string    // 分析密码
	expired time.Time // 过期时间
}
