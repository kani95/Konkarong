using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _dataContext;

    public PhotoRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<Photo> GetPhotoById(int id)
    {
        return await _dataContext.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PhotoForApprovalDTO>> GetUnapprovedPhotos()
    {
        return await _dataContext.Photos
            .IgnoreQueryFilters()
            .Where(p => p.IsApproved == false)
            .Select(p => new PhotoForApprovalDTO
            {
                Id = p.Id,
                Username = p.AppUser.UserName,
                Url = p.Url,
                IsApproved = p.IsApproved
            }).ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        _dataContext.Photos.Remove(photo);
    }
}
