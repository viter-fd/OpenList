using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenList.Application.DTOs;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;

namespace OpenList.Api.Controllers;

/// <summary>
/// 存储控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly IStorageRepository _storageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StorageController> _logger;

    public StorageController(IStorageRepository storageRepository, IUnitOfWork unitOfWork, ILogger<StorageController> logger)
    {
        _storageRepository = storageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有存储
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<StorageDto>>>> GetAll()
    {
        try
        {
            var storages = await _storageRepository.GetAllAsync();
            var storageDtos = storages.Select(MapToDto)
            {
                Id = s.Id,
                MountPath = s.MountPath,
                Name = s.Name,
                Driver = s.Driver,
                Order = s.Order,
                IsEnabled = s.IsEnabled,
                Remark = s.Remark
            });

            return Ok(ApiResponse<IEnumerable<StorageDto>>.Ok(storageDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storages");
            return StatusCode(500, ApiResponse<IEnumerable<StorageDto>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 根据ID获取存储
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StorageDto>>> GetById(long id)
    {
        try
        {
            var storage = await _storageRepository.GetByIdAsync(id);
            if (storage == null)
            {
                return NotFound(ApiResponse<StorageDto>.Fail("Storage not found"));
            }

            var storageDto = new StorageDto
            {
                Id = storage.Id,
                MountPath = storage.MountPath,
                Name = storage.Name,
                Driver = storage.Driver,
                Order = storage.Order,
                IsEnabled = storage.IsEnabled,
                Remark = storage.Remark
            };

            return Ok(ApiResponse<StorageDto>.Ok(storageDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage {Id}", id);
            return StatusCode(500, ApiResponse<StorageDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 创建存储
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StorageDto>>> Create([FromBody] CreateStorageRequest request)
    {
        try
        {
            // 检查挂载路径是否已存在
            var existing = await _storageRepository.GetByMountPathAsync(request.MountPath);
            if (existing != null)
            {
                return BadRequest(ApiResponse<StorageDto>.Fail("Mount path already exists"));
            }

            var storage = new Storage
            {
                MountPath = request.MountPath,
                Name = request.Name,
                Driver = request.Driver,
                ConfigJson = request.ConfigJson,
                Order = request.Order,
                Remark = request.Remark,
                IsEnabled = true
            };

            await _storageRepository.AddAsync(storage);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = storage.Id }, ApiResponse<StorageDto>.Ok(MapToDto(storage)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating storage");
            return StatusCode(500, ApiResponse<StorageDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 更新存储
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<StorageDto>>> Update(long id, [FromBody] CreateStorageRequest request)
    {
        try
        {
            var storage = await _storageRepository.GetByIdAsync(id);
            if (storage == null)
            {
                return NotFound(ApiResponse<StorageDto>.Fail("Storage not found"));
            }

            storage.MountPath = request.MountPath;
            storage.Name = request.Name;
            storage.Driver = request.Driver;
            storage.ConfigJson = request.ConfigJson;
            storage.Order = request.Order;
            storage.Remark = request.Remark;

            _storageRepository.Update(storage);
            await _unitOfWork.SaveChangesAsync();

            var storageDto = new StorageDto
            {
                Id = storage.Id,
                MountPath = storage.MountPath,
                Name = storage.Name,
                Driver = storage.Driver,
                Order = storage.Order,
                IsEnabled = storage.IsEnabled,
                Remark = storage.Remark
            };

            return Ok(ApiResponse<StorageDto>.Ok(storageDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating storage {Id}", id);
            return StatusCode(500, ApiResponse<StorageDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 删除存储
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        try
        {
            var storage = await _storageRepository.GetByIdAsync(id);
            if (storage == null)
            {
                return NotFound(ApiResponse<bool>.Fail("Storage not found"));
            }

            _storageRepository.Delete(storage);
            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Storage deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting storage {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// 启用/禁用存储
    /// </summary>
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<ApiResponse<bool>>> Toggle(long id)
    {
        try
        {
            var storage = await _storageRepository.GetByIdAsync(id);
            if (storage == null)
            {
                return NotFound(ApiResponse<bool>.Fail("Storage not found"));
            }

            storage.IsEnabled = !storage.IsEnabled;
            _storageRepository.Update(storage);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, $"Storage {(storage.IsEnabled ? "enabled" : "disabled")} successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling storage {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Fail("Internal server error"));
        }
    }

    private static StorageDto MapToDto(Storage storage)
    {
        return new StorageDto
        {
            Id = storage.Id,
            MountPath = storage.MountPath,
            Name = storage.Name,
            Driver = storage.Driver,
            ConfigJson = storage.ConfigJson,
            Order = storage.Order,
            IsEnabled = storage.IsEnabled,
            Remark = storage.Remark,
            CreatedAt = storage.CreatedAt,
            UpdatedAt = storage.UpdatedAt
        };
    }
}
