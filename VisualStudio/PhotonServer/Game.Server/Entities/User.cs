using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Server.Entities.ValueObjects;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Game.Server.Entities
{
    public class User
    {
        public virtual int Id { get; set; }
        public virtual string Email { get; set; }
        public virtual string Username { get; set; }
        public virtual HashedPassword Password { get; set; }
        
        public virtual DateTime CreatedAt { get; set; }
    }

    public class UserMap : ClassMapping<User>
    {
        public UserMap()
        {
            Table("users");
            
            Id(x=>x.Id, x=>x.Generator(Generators.Identity));

            Property(x=>x.Email, x=>x.NotNullable(true));
            Property(x=>x.Username, x=>x.NotNullable(true));

            Component(x=>x.Password, y =>
            {
                y.Property(x => x.Salt, z =>
                {
                    z.Column("password_salt");
                    z.NotNullable(true);
                });

                y.Property(x=>x.Hash, z =>
                {
                    z.Column("password_hash");
                    z.NotNullable(true);
                });

            });
            Property(x=>x.CreatedAt, x =>
            {
                x.Column("created_at");
                x.NotNullable(true);
            });
        }
    }
}
