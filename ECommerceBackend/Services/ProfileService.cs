using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services
{
    public class ProfileService
    {
        private readonly AppDbContext _context;

        public ProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileDto?> GetProfileAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new ProfileDto
                {
                    Name = u.Name,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<AddressDto>> GetAddressesAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new AddressDto
                {
                    FullName = a.FullName,
                    Phone = a.Phone,
                    Line1 = a.Line1,
                    Line2 = a.Line2,
                    City = a.City,
                    State = a.State,
                    Pincode = a.Pincode,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();
        }

        public async Task<AddressDto> AddAddressAsync(int userId, AddressDto dto)
        {
            var hasAddresses = await _context.Addresses// does the user has atleast one address
                .AnyAsync(a => a.UserId == userId);

            if (!hasAddresses)// if there is no address on the table, we make the address we are adding now as default address
            {// if there is an existing address, this wont execute
                dto.IsDefault = true;
            }

            // address is already exisiting
            // if the one more address we are adding consists a value as default:true, we are making the one more address as default
            // we make the exisiting default address as default:false
            if (dto.IsDefault)
            {
                var existingDefaultAddress = await _context.Addresses// Finding the user's existing default addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

                if (existingDefaultAddress != null)
                {
                    existingDefaultAddress.IsDefault = false;
                }
            }

            var addressEntity = new Address
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Line1 = dto.Line1,
                Line2 = dto.Line2,
                City = dto.City,
                State = dto.State,
                Pincode = dto.Pincode,
                IsDefault = dto.IsDefault
            };

            _context.Addresses.Add(addressEntity);

            await _context.SaveChangesAsync();

            return dto;
        }

        public async Task<bool> UpdateAddressAsync(int userId, int addressId, AddressDto dto)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
            {
                return false;
            }

            if (dto.IsDefault)
            {
                var existingDefaultAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault && a.Id != addressId);

                if (existingDefaultAddress != null)
                {
                    existingDefaultAddress.IsDefault = false;
                }
            }

            address.FullName = dto.FullName;
            address.Phone = dto.Phone;
            address.Line1 = dto.Line1;
            address.Line2 = dto.Line2;
            address.City = dto.City;
            address.State = dto.State;
            address.Pincode = dto.Pincode;
            address.IsDefault = dto.IsDefault;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
            {
                return false;
            }

            var wasDefault = address.IsDefault;

            _context.Addresses.Remove(address);

            if (wasDefault)
            {
                var newDefaultAddress = await _context.Addresses
                    .Where(a => a.UserId == userId && a.Id != addressId)
                    .OrderBy(a => a.Id)
                    .FirstOrDefaultAsync();

                if (newDefaultAddress != null)
                {
                    newDefaultAddress.IsDefault = true;
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a =>
                    a.Id == addressId &&
                    a.UserId == userId);

            if (address == null)
            {
                return false;
            }

            if (address.IsDefault)
            {
                return true;
            }

            var existingDefaultAddress = await _context.Addresses
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.IsDefault);

            if (existingDefaultAddress != null)
            {
                existingDefaultAddress.IsDefault = false;
            }

            address.IsDefault = true;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}