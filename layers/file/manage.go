package file

import (
	"context"
)

// UserFileServer 文件服务接口 #################################################################
type UserFileServer interface {
	// CopyFile 复制文件 =======================================================================
	CopyFile(ctx context.Context, sources []string, targets []string) ([]*BackFileAction, error)
	// MoveFile 移动文件 =======================================================================
	MoveFile(ctx context.Context, sources []string, targets []string) ([]*BackFileAction, error)
	// NameFile 移动文件 =======================================================================
	NameFile(ctx context.Context, sources []string, targets []string) ([]*BackFileAction, error)
	// ListFile 列举文件 =======================================================================
	ListFile(ctx context.Context, path []string, opt *ListFileOption) ([]*UserFileObject, error)
	// FindFile 搜索文件 =======================================================================
	FindFile(ctx context.Context, path []string, opt *FindFileOption) ([]*UserFileObject, error)
	// Download 获取文件 =======================================================================
	Download(ctx context.Context, path []string, opt *ListFileOption) ([]*LinkFileObject, error)
	// Uploader 上传文件 =======================================================================
	Uploader(ctx context.Context, path []string, opt *ListFileOption) ([]*BackFileAction, error)
	// KillFile 删除文件 =======================================================================
	KillFile(ctx context.Context, path []string, opt *KillFileOption) ([]*BackFileAction, error)
	// MakeFile 搜索文件 =======================================================================
	MakeFile(ctx context.Context, path []string, opt *MakeFileOption) ([]*BackFileAction, error)
	// MakePath 搜索文件 =======================================================================
	MakePath(ctx context.Context, path []string, opt *MakeFileOption) ([]*BackFileAction, error)
	// PermFile 设置权限 =======================================================================
	PermFile(ctx context.Context, path []string, opt *PermissionFile) ([]*BackFileAction, error)
	// NewShare 创建分享 =======================================================================
	NewShare(ctx context.Context, path []string, opt *NewShareAction) ([]*BackFileAction, error)
	// GetShare 获取分享 =======================================================================
	GetShare(ctx context.Context, path []string, opt *NewShareAction) ([]*UserFileObject, error)
	// DelShare 删除分享 =======================================================================
	DelShare(ctx context.Context, path []string, opt *NewShareAction) ([]*BackFileAction, error)
}

type UserFileUpload interface {
	fullPost(ctx context.Context, path []string)
	pfCreate(ctx context.Context, path []string)
	pfUpload(ctx context.Context, path []string)
	pfUpdate(ctx context.Context, path []string)
}

func ListFile(ctx context.Context, path []string, opt *ListFileOption) ([]*UserFileObject, error) {
	originList := make([]*HostFileObject, 0)
	serverList := make([]*UserFileObject, 0)
	for _, fileItem := range originList {
		serverList = append(serverList, &UserFileObject{
			HostFileObject: *fileItem,
		})
	}
	return serverList, nil
}
