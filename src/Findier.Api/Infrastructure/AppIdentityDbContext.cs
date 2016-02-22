using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Validation;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Findier.Api.Infrastructure
{
    /// <summary>
    ///     From the entity framework, needed to add a constructor that allowed using an object context.
    /// </summary>
    public class AppIdentityDbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> : DbContext
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>
        where TRole : IdentityRole<TKey, TUserRole>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
    {
        public AppIdentityDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        /// <summary>
        ///     Default constructor which uses the "DefaultConnection" connectionString
        /// </summary>
        public AppIdentityDbContext()
            : this("DefaultConnection")
        {
        }

        /// <summary>
        ///     Constructor which takes the connection string to use
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        public AppIdentityDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        /// <summary>
        ///     Constructs a new context instance using the existing connection to connect to a database, and initializes it from
        ///     the given model.  The connection will not be disposed when the context is disposed if contextOwnsConnection is
        ///     false.
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context.</param>
        /// <param name="model">The model that will back this context.</param>
        /// <param name="contextOwnsConnection">
        ///     Constructs a new context instance using the existing connection to connect to a
        ///     database, and initializes it from the given model.  The connection will not be disposed when the context is
        ///     disposed if contextOwnsConnection is false.
        /// </param>
        public AppIdentityDbContext(
            DbConnection existingConnection,
            DbCompiledModel model,
            bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        /// <summary>
        ///     Constructs a new context instance using conventions to create the name of
        ///     the database to which a connection will be made, and initializes it from
        ///     the given model.  The by-convention name is the full name (namespace + class
        ///     name) of the derived context class.  See the class remarks for how this is
        ///     used to create a connection.
        /// </summary>
        /// <param name="model">The model that will back this context.</param>
        public AppIdentityDbContext(DbCompiledModel model)
            : base(model)
        {
        }

        /// <summary>
        ///     Constructs a new context instance using the existing connection to connect
        ///     to a database.  The connection will not be disposed when the context is disposed
        ///     if contextOwnsConnection is false.
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context.</param>
        /// <param name="dbContextOwnsObjectContext">
        ///     If set to true the connection is disposed when the context is disposed, otherwise
        ///     the caller must dispose the connection.
        /// </param>
        public AppIdentityDbContext(DbConnection existingConnection, bool dbContextOwnsObjectContext)
            : base(existingConnection, dbContextOwnsObjectContext)
        {
        }

        /// <summary>
        ///     Constructs a new context instance using the given string as the name or connection
        ///     string for the database to which a connection will be made, and initializes
        ///     it from the given model.  See the class remarks for how this is used to create
        ///     a connection.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        /// <param name="model">The model that will back this context.</param>
        public AppIdentityDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        /// <summary>
        ///     If true validates that emails are unique
        /// </summary>
        public bool RequireUniqueEmail { get; set; }

        /// <summary>
        ///     IDbSet of Roles
        /// </summary>
        public virtual IDbSet<TRole> Roles { get; set; }

        /// <summary>
        ///     IDbSet of Users
        /// </summary>
        public virtual IDbSet<TUser> Users { get; set; }

        /// <summary>
        ///     Maps table names, and sets up relationships between the various user entities
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(DbModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Needed to ensure subclasses share the same table
            var user = builder.Entity<TUser>()
                .ToTable("Users").HasKey(p => p.Id);
            user.HasMany(u => u.Roles).WithRequired().HasForeignKey(ur => ur.UserId);
            user.HasMany(u => u.Claims).WithRequired().HasForeignKey(uc => uc.UserId);
            user.HasMany(u => u.Logins).WithRequired().HasForeignKey(ul => ul.UserId);

            // CONSIDER: u.Email is Required if set on options?
            user.Property(u => u.Email);

            builder.Entity<TUserRole>()
                .HasKey(r => new { r.UserId, r.RoleId })
                .ToTable("AppUserRoles");

            builder.Entity<TUserLogin>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("AppUserLogins");

            builder.Entity<TUserClaim>()
                .ToTable("AppUserClaims");

            var role = builder.Entity<TRole>()
                .ToTable("AppRoles");
            role.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("RoleNameIndex") { IsUnique = true }));
            role.HasMany(r => r.Users).WithRequired().HasForeignKey(ur => ur.RoleId);
        }

        /// <summary>
        ///     Validates that UserNames are unique and case insenstive
        /// </summary>
        /// <param name="entityEntry"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        protected override DbEntityValidationResult ValidateEntity(
            DbEntityEntry entityEntry,
            IDictionary<object, object> items)
        {
            if (entityEntry == null || entityEntry.State != EntityState.Added)
            {
                return base.ValidateEntity(entityEntry, items);
            }
            var errors = new List<DbValidationError>();
            var user = entityEntry.Entity as TUser;
            //check for uniqueness of user email
            if (user != null)
            {

                if (RequireUniqueEmail && Users.Any(u => string.Equals(u.Email, user.Email)))
                {
                    errors.Add(new DbValidationError("User", "Email is already being used."));
                }
            }
            else
            {
                var role = entityEntry.Entity as TRole;
                //check for uniqueness of role name
                if (role != null && Roles.Any(r => string.Equals(r.Name, role.Name)))
                {
                    errors.Add(new DbValidationError("Role", "The role already exists."));
                }
            }
            return errors.Any() ? new DbEntityValidationResult(entityEntry, errors) : base.ValidateEntity(entityEntry, items);
        }
    }
}