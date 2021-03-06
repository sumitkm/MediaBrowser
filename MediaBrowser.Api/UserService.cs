﻿using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Users;
using ServiceStack.ServiceHost;
using ServiceStack.Text.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBrowser.Api
{
    /// <summary>
    /// Class GetUsers
    /// </summary>
    [Route("/Users", "GET")]
    [Api(Description = "Gets a list of users")]
    public class GetUsers : IReturn<List<UserDto>>
    {
        [ApiMember(Name = "IsHidden", Description="Optional filter by IsHidden=true or false", IsRequired = false, DataType = "bool", ParameterType = "query", Verb = "GET")]
        public bool? IsHidden { get; set; }

        [ApiMember(Name = "IsDisabled", Description = "Optional filter by IsDisabled=true or false", IsRequired = false, DataType = "bool", ParameterType = "query", Verb = "GET")]
        public bool? IsDisabled { get; set; }
    }

    [Route("/Users/Public", "GET")]
    [Api(Description = "Gets a list of publicly visible users for display on a login screen.")]
    public class GetPublicUsers : IReturn<List<UserDto>>
    {
    }
    
    /// <summary>
    /// Class GetUser
    /// </summary>
    [Route("/Users/{Id}", "GET")]
    [Api(Description = "Gets a user by Id")]
    public class GetUser : IReturn<UserDto>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Class DeleteUser
    /// </summary>
    [Route("/Users/{Id}", "DELETE")]
    [Api(Description = "Deletes a user")]
    public class DeleteUser : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "DELETE")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Class AuthenticateUser
    /// </summary>
    [Route("/Users/{Id}/Authenticate", "POST")]
    [Api(Description = "Authenticates a user")]
    public class AuthenticateUser : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [ApiMember(Name = "Password", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Class AuthenticateUser
    /// </summary>
    [Route("/Users/{Name}/AuthenticateByName", "POST")]
    [Api(Description = "Authenticates a user")]
    public class AuthenticateUserByName : IReturn<AuthenticationResult>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Name", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [ApiMember(Name = "Password", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Class UpdateUserPassword
    /// </summary>
    [Route("/Users/{Id}/Password", "POST")]
    [Api(Description = "Updates a user's password")]
    public class UpdateUserPassword : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string CurrentPassword { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        /// <value>The new password.</value>
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reset password].
        /// </summary>
        /// <value><c>true</c> if [reset password]; otherwise, <c>false</c>.</value>
        public bool ResetPassword { get; set; }
    }

    /// <summary>
    /// Class UpdateUser
    /// </summary>
    [Route("/Users/{Id}", "POST")]
    [Api(Description = "Updates a user")]
    public class UpdateUser : UserDto, IReturnVoid
    {
    }

    /// <summary>
    /// Class CreateUser
    /// </summary>
    [Route("/Users", "POST")]
    [Api(Description = "Creates a user")]
    public class CreateUser : UserDto, IReturn<UserDto>
    {
    }

    /// <summary>
    /// Class UsersService
    /// </summary>
    public class UserService : BaseApiService
    {
        /// <summary>
        /// The _XML serializer
        /// </summary>
        private readonly IXmlSerializer _xmlSerializer;

        /// <summary>
        /// The _user manager
        /// </summary>
        private readonly IUserManager _userManager;
        private readonly ILibraryManager _libraryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService" /> class.
        /// </summary>
        /// <param name="xmlSerializer">The XML serializer.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <exception cref="System.ArgumentNullException">xmlSerializer</exception>
        public UserService(IXmlSerializer xmlSerializer, IUserManager userManager, ILibraryManager libraryManager)
            : base()
        {
            if (xmlSerializer == null)
            {
                throw new ArgumentNullException("xmlSerializer");
            }

            _xmlSerializer = xmlSerializer;
            _userManager = userManager;
            _libraryManager = libraryManager;
        }

        public object Get(GetPublicUsers request)
        {
            return Get(new GetUsers
            {
                IsHidden = false,
                IsDisabled = false
            });
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetUsers request)
        {
            var dtoBuilder = new UserDtoBuilder(Logger);

            var users = _userManager.Users;

            if (request.IsDisabled.HasValue)
            {
                users = users.Where(i => i.Configuration.IsDisabled == request.IsDisabled.Value);
            }

            if (request.IsHidden.HasValue)
            {
                users = users.Where(i => i.Configuration.IsHidden == request.IsHidden.Value);
            }

            var tasks = users.OrderBy(u => u.Name).Select(dtoBuilder.GetUserDto).Select(i => i.Result);

            return ToOptimizedResult(tasks.ToList());
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetUser request)
        {
            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            var dtoBuilder = new UserDtoBuilder(Logger);

            var result = dtoBuilder.GetUserDto(user).Result;

            return ToOptimizedResult(result);
        }

        /// <summary>
        /// Deletes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Delete(DeleteUser request)
        {
            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            var task = _userManager.DeleteUser(user);

            Task.WaitAll(task);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(AuthenticateUser request)
        {
            // No response needed. Will throw an exception on failure.
            var result = AuthenticateUser(request).Result;
        }

        public object Post(AuthenticateUserByName request)
        {
            var user = _userManager.Users.FirstOrDefault(i => string.Equals(request.Name, i.Name, StringComparison.OrdinalIgnoreCase));

            var result = AuthenticateUser(new AuthenticateUser { Id = user.Id, Password = request.Password }).Result;

            return ToOptimizedResult(result);
        }

        private async Task<AuthenticationResult> AuthenticateUser(AuthenticateUser request)
        {
            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            var success = await _userManager.AuthenticateUser(user, request.Password).ConfigureAwait(false);

            if (!success)
            {
                // Unauthorized
                throw new UnauthorizedAccessException("Invalid user or password entered.");
            }

            var result = new AuthenticationResult
            {
                User = await new UserDtoBuilder(Logger).GetUserDto(user).ConfigureAwait(false)
            };

            return result;
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(UpdateUserPassword request)
        {
            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            if (request.ResetPassword)
            {
                var task = _userManager.ResetPassword(user);

                Task.WaitAll(task);
            }
            else
            {
                var success = _userManager.AuthenticateUser(user, request.CurrentPassword).Result;

                if (!success)
                {
                    throw new UnauthorizedAccessException("Invalid user or password entered.");
                }

                var task = _userManager.ChangePassword(user, request.NewPassword);

                Task.WaitAll(task);
            }
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(UpdateUser request)
        {
            // We need to parse this manually because we told service stack not to with IRequiresRequestStream
            // https://code.google.com/p/servicestack/source/browse/trunk/Common/ServiceStack.Text/ServiceStack.Text/Controller/PathInfo.cs
            var pathInfo = PathInfo.Parse(RequestContext.PathInfo);
            var id = new Guid(pathInfo.GetArgumentValue<string>(1));

            var dtoUser = request;

            var user = _userManager.GetUserById(id);

            // If removing admin access
            if (!dtoUser.Configuration.IsAdministrator && user.Configuration.IsAdministrator)
            {
                if (_userManager.Users.Count(i => i.Configuration.IsAdministrator) == 1)
                {
                    throw new ArgumentException("There must be at least one user in the system with administrative access.");
                }
            }

            // If disabling
            if (dtoUser.Configuration.IsDisabled && user.Configuration.IsAdministrator)
            {
                throw new ArgumentException("Administrators cannot be disabled.");
            }

            // If disabling
            if (dtoUser.Configuration.IsDisabled && !user.Configuration.IsDisabled)
            {
                if (_userManager.Users.Count(i => !i.Configuration.IsDisabled) == 1)
                {
                    throw new ArgumentException("There must be at least one enabled user in the system.");
                }
            }

            var task = user.Name.Equals(dtoUser.Name, StringComparison.Ordinal) ? _userManager.UpdateUser(user) : _userManager.RenameUser(user, dtoUser.Name);

            Task.WaitAll(task);

            user.UpdateConfiguration(dtoUser.Configuration, _xmlSerializer);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Post(CreateUser request)
        {
            var dtoUser = request;

            var newUser = _userManager.CreateUser(dtoUser.Name).Result;

            newUser.UpdateConfiguration(dtoUser.Configuration, _xmlSerializer);

            var dtoBuilder = new UserDtoBuilder(Logger);

            var result = dtoBuilder.GetUserDto(newUser).Result;

            return ToOptimizedResult(result);
        }
    }
}
