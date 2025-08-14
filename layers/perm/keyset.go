package perm

type UserKeys struct {
	uuid    string   // 密钥UUID
	user    string   // 所属用户
	main    string   // 核心密钥（用户密钥SHA2）
	name    string   // 友好名称
	algo    int8     // 密钥算法
	enabled bool     // 是否启用
	encFile bool     // 加密文件
	encName bool     // 加密名称
	keyAuto bool     // 自动更新
	keyRand bool     // 随机密钥
	keyAuth UserAuth // 密钥认证
}

type UserAuth struct {
	uuid   string // 密钥UUID
	user   string // 所属用户
	plugin string // 认证插件
	config string // 认证配置
}
