package conf

import "regexp"

var (
	Conf     *Config
	SitePath string
)

var PrivacyReg []*regexp.Regexp
