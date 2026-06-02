using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace ApiNexusERP.Helpers
{
    public class DateOnlyModelCustomizer : RelationalModelCustomizer
    {
        public DateOnlyModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateOnly))
                    {
                        property.SetValueConverter(new ValueConverter<DateOnly, DateTime>(
                            v => v.ToDateTime(TimeOnly.MinValue),
                            v => DateOnly.FromDateTime(v)
                        ));
                    }
                    else if (property.ClrType == typeof(DateOnly?))
                    {
                        property.SetValueConverter(new ValueConverter<DateOnly?, DateTime?>(
                            v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : null,
                            v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null
                        ));
                    }
                }
            }
        }
    }
}
