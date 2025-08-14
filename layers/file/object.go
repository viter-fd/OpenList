package file

import "time"

type UserFileObject struct {
	showName []string  // 文件名称
	showPath []string  // 文件路径
	realName []string  // 真实名称
	realPath []string  // 真实路径
	fileSize int64     // 文件大小
	fileTime time.Time // 修改时间
	fileType bool      // 文件类型
	fileMask int16     // 文件权限
	checksum int32     // 密码校验
	encrypts int16     // 文件状态
}
type PermissionFile struct {
}

type LinkFileObject struct {
	download []string // 下载链接

}

type ListFileOption struct {
}

type FindFileOption struct {
}

type KillFileOption struct {
}
type MakeFileOption struct {
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
type HostFileObject struct {
	UserFileObject
}
