package perm

type ShareUrl struct {
	uuid string // 密钥UUID
	user string // 所属用户
	path string // 分享路径
	pass string // 分享密码
	date string // 过期时间
	flag bool   // 是否有效
}
