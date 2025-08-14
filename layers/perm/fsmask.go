package perm

type FileMask struct {
	uuid     string // 密钥UUID
	user     string // 所属用户
	path     string // 匹配路径
	name     string // 友好名称
	idKeyset string // 密钥集ID
	encrypts string // 加密组ID
	password string // 独立密码
	fileUser string // 所有用户
	filePart int64  // 分卷大小
	fileMask int16  // 文件权限
	compress int16  // 是否压缩
	isEnable bool   // 是否启用
}
