package user

type UserInfo struct {
	uuid string   // 用户UUID
	name string   // 用户名称
	flag bool     // 是否有效
	perm PermInfo // 权限信息
}

type PermInfo struct {
	isAdmin bool // 是否管理员
	davRead bool // 是否允许读
	// ...
}
