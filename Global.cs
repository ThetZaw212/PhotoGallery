global using PhotoGallery.Data;
global using PhotoGallery.Interface;
global using PhotoGallery.Extensions;
global using PhotoGallery.Services;
global using PhotoGallery.Helper;
global using PhotoGallery.Models;
global using PhotoGallery.Entities;

global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authentication.JwtBearer;

global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;

namespace PhotoGallery
{
    public class GLOBAL
    {
        public static bool IsOriginAllowed(string origin)
        {
            Uri uri = new(origin);
            _ = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "n/a";

            bool isAllowed = uri.Host.Equals("", StringComparison.OrdinalIgnoreCase)
                             || uri.Host.Equals("", StringComparison.OrdinalIgnoreCase);

            if (!isAllowed)
                isAllowed = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);

            return isAllowed;
        }
    }
}
