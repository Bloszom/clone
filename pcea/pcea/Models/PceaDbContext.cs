using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pcea.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using pcea.Models;
using pceaLibrary;

namespace pcea.Models
{
    public partial class PceaDbContext : DbContext
    {
        public IConfiguration Configuration { get; }
        AuditTrail _AuditTrail;
        Vars _Vars;

        public PceaDbContext()
        {
        }

        public PceaDbContext(DbContextOptions<PceaDbContext> options, IWebHostEnvironment environment, IHttpContextAccessor httpContext)
            : base(options)
        {
            _AuditTrail = new AuditTrail(environment);
            _Vars = new Vars(httpContext.HttpContext);
        }

        public class Obj
        {
            public string TableName { get; set; }
            public string Name { get; set; }
            public object CurrentValue { get; set; }
            public object OriginalValue { get; set; }
        };

        public async override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default)
        {
            
            //var _Context = finadsDbContext();
            var changeInfo = this.ChangeTracker.Entries()
                .Where(t => t.State == EntityState.Modified)
                .Select(t => new {
                    Original = t.OriginalValues.Properties.ToDictionary(pn => pn.Name, pn => t.OriginalValues[pn]),
                    Current = t.CurrentValues.Properties.ToDictionary(pn => pn.Name, pn => t.CurrentValues[pn]),
                });
            var jsonData = JsonConvert.SerializeObject(changeInfo);
            try
            {
                _AuditTrail.LogToFile(new LogItem
                {
                    USER_ID = _Vars.UserId,
                    NAME = _Vars.FullName,
                    ACCESSED_MODULE = "Same as above",
                    OPERATORNAME = _Vars.OperatorName,
                    ACTIVITY_DETAIL = jsonData,
                    ACTIVITY_TIME = DateTime.Now,
                    USER_TYPE = _Vars.UserType
                });
            }
            catch (Exception)
            {

            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            //var _Context = finadsDbContext();
            var entries = this.ChangeTracker.Entries().Select(e => e.Members);
            var _obj = new List<Obj>();
            //var _obj = new Obj() { 
            //    CurrentValue = fieldNames.Select(e => e.Select(f => f.CurrentValue)),
            //    Name = fieldNames.Select(e => e.Select(f => f.Metadata.Name))
            //};

            //Iterate through the entries
            //foreach (var entry in entries)
            //{
            //    foreach (var _entry in entry) 
            //    { 

            //        foreach (var item in entry.Select(e => e.EntityEntry.OriginalValues.Properties.ToDictionary(pn => pn.Name, pn => e.EntityEntry.CurrentValues[pn])))
            //        {

            //        }

            //        _obj.Add(new Obj()
            //        {
            //            TableName = _entry.Metadata.DeclaringType.ShortName(),
            //            Name = _entry.Metadata.Name,
            //            CurrentValue = _entry.CurrentValue,
            //            OriginalValue = entry.Select(e => e.EntityEntry.OriginalValues.Properties.ToDictionary(pn => pn, pn => e.EntityEntry.CurrentValues[pn]))
            //        });
            //    }

            //}

            //Idea - can the entries be grouped by the table names?

            var changeInfo = this.ChangeTracker.Entries()
                .Where(t => t.State == EntityState.Modified || t.State == EntityState.Added)
                .Select(t => new
                {
                    Original = t.OriginalValues.Properties.ToDictionary(pn => pn.Name, pn => t.OriginalValues[pn]),
                    Current = t.CurrentValues.Properties.ToDictionary(pn => pn.Name, pn => t.CurrentValues[pn]),
                });

            var jsonData = JsonConvert.SerializeObject(changeInfo);
            _AuditTrail.LogToFile(new LogItem
            {
                USER_ID = _Vars.UserId,
                NAME = _Vars.FullName,
                ACCESSED_MODULE = "Same as above",
                OPERATORNAME = _Vars.OperatorName,
                ACTIVITY_DETAIL = jsonData,
                ACTIVITY_TIME = DateTime.Now,
                USER_TYPE = _Vars.UserType
            });

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public virtual DbSet<Analysis> Analysis { get; set; }
        public virtual DbSet<AppPrivilege> AppPrivilege { get; set; }
        public virtual DbSet<AppRole> AppRole { get; set; }
        public virtual DbSet<AppUserProfileViewV2> AppUserProfileView { get; set; }
        public virtual DbSet<CompanyDataSubmission> CompanyDataSubmissions { get; set; }
        public virtual DbSet<Forms> Forms { get; set; }
        public virtual DbSet<FormsAndEntry> FormsAndEntry { get; set; }
        public virtual DbSet<FormsDetails> FormsDetails { get; set; }
        public virtual DbSet<FormsEntry> FormsEntry { get; set; }
        public virtual DbSet<FormsEntryDTO> FormsEntryDTO { get; set; }
        public virtual DbSet<FormsOperators> FormsOperators { get; set; }
        public virtual DbSet<FormsSubmission> FormsSubmission { get; set; }
        public virtual DbSet<FormsMessage> FormsMessage { get; set; }
        public virtual DbSet<FormsReview> FormsReview { get; set; }
        public virtual DbSet<MailConfig> MailConfig { get; set; }
        public virtual DbSet<MailMessage> MailMessage { get; set; }
        public virtual DbSet<MetaData> MetaData { get; set; }
        public virtual DbSet<MetaDataRef> MetaDataRef { get; set; }
        public virtual DbSet<ReportOperatorEntry> ReportOperatorEntry { get; set; }
        public virtual DbSet<Reports> Reports { get; set; }
        public virtual DbSet<ReportTemplate> ReportTemplate { get; set; }
        public virtual DbSet<ReportsCategory> ReportsCategory { get; set; }
        public virtual DbSet<SsoInvoice> SsoInvoice { get; set; }
        public virtual DbSet<SsoToken> SsoToken { get; set; }
        public virtual DbSet<SsoUser> SsoUser { get; set; }
        public virtual DbSet<SystemConfig> SystemConfig { get; set; }
        public virtual DbSet<TariffAnalysis> TariffAnalysis { get; set; }
        public virtual DbSet<TariffAnalysisReport> TariffAnalysisReport { get; set; }
        public virtual DbSet<TariffEvaluation> TariffEvaluation { get; set; }
        public virtual DbSet<TariffHistories> TariffHistories { get; set; }
        public virtual DbSet<UserPrivilege> UserPrivilege { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }
        public virtual DbSet<UserSession> UserSession { get; set; }
        public virtual DbSet<Workflow> Workflow { get; set; }
        public virtual DbSet<WorkflowActor> WorkflowActor { get; set; }
        public virtual DbSet<WorkflowLink> WorkflowLink { get; set; }
        public virtual DbSet<WorkflowManager> WorkflowManager { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("mssqlConnectionString"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasAnnotation("Relational:DefaultSchema", "cloudwar_pcea");

            //modelBuilder.Entity<RoleViewModel>()
            //    .HasNoKey();

            modelBuilder.Entity<Analysis>(entity =>
            {

                entity.Property(e => e.ReportId).IsUnicode(false);

                entity.Property(e => e.ReportDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ReportValue).IsUnicode(false);

                entity.Property(e => e.Operator).IsUnicode(false);

                entity.Property(e => e.OperatorType).IsUnicode(false);

                entity.Property(e => e.Analyst).IsUnicode(false);

                entity.Property(e => e.Year).IsUnicode(false);

                entity.Property(e => e.EntryDate).HasDefaultValueSql("(getdate())");

            });

            modelBuilder.Entity<AppPrivilege>(entity =>
            {
                entity.Property(e => e.PrivilegeId).ValueGeneratedNever();

                entity.Property(e => e.ActionName).IsUnicode(false);

                entity.Property(e => e.ControllerName).IsUnicode(false);

                entity.Property(e => e.Description).IsUnicode(false);
            });

            modelBuilder.Entity<AppRole>(entity =>
            {
                entity.Property(e => e.RoleId).IsUnicode(false);

                entity.Property(e => e.RoleNo).IsUnicode(false).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.RoleName).IsUnicode(false);
            });

            modelBuilder.Entity<AppUserProfileViewV2>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("AppUserProfileView");
            });            
            
            modelBuilder.Entity<CompanyDataSubmission>(entity =>
            {
                entity.Property(e => e.FormId).IsUnicode(false);

                entity.Property(e => e.FormFieldsData).IsUnicode(false);

            });

            modelBuilder.Entity<Forms>(entity =>
            {
                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FormFields).IsUnicode(false);

                entity.Property(e => e.FormName).IsUnicode(false);

                entity.Property(e => e.FormType).IsUnicode(false);

                entity.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FormsType).IsUnicode(false);

                entity.Property(e => e.FormsTypeCategory).IsUnicode(false);

                entity.Property(e => e.Published).HasDefaultValueSql("((0))");

                entity.Property(e => e.UpdateCount).HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).IsUnicode(false);

                entity.Property(e => e.FormYear).IsUnicode(false);

                entity.Property(e => e.TerminalDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProcessId).IsUnicode(false);

                entity.Property(e => e.CompanyInfoFields).IsUnicode(false);

            });

            modelBuilder.Entity<FormsDetails>(entity =>
            {
                entity.Property(e => e.AppType)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('NEW')");

                entity.Property(e => e.OperatorId).IsUnicode(false);

                entity.Property(e => e.FormId).IsUnicode(false);

                entity.Property(e => e.FormDetails).IsUnicode(false);

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EntryId).IsUnicode(false);

                entity.Property(e => e.MasterEntryId).IsUnicode(false);

                entity.Property(e => e.OldEntryId).IsUnicode(false);

                entity.Property(e => e.LicenseCategory).IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('PENDING')");

            });

            modelBuilder.Entity<FormsReview>(entity =>
            {
                entity.Property(e => e.FileUrl).IsUnicode(false);

                entity.Property(e => e.Remarks).IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('PENDING')");
            });

            modelBuilder.Entity<FormsSubmission>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("FormsSubmission");
            });

            modelBuilder.Entity<FormsOperators>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("FormsOperators");
            });

            modelBuilder.Entity<FormsAndEntry>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("FormsAndEntry");
            });
            
            modelBuilder.Entity<FormsEntry>(entity =>
            {
                entity.Property(e => e.DateSubmitted).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FieldName).IsUnicode(false);

                entity.Property(e => e.OperatorId).IsUnicode(false);

                entity.Property(e => e.Response).IsUnicode(false);

                entity.Property(e => e.UserId).IsUnicode(false);

                entity.Property(e => e.EntryId).IsUnicode(false);

                entity.Property(e => e.Status).IsUnicode(false);

                entity.Property(e => e.FieldLabel).IsUnicode(false);

                entity.Property(e => e.IsFile).IsUnicode(false);

                entity.Property(e => e.FrmYear).IsUnicode(false);

                entity.Property(e => e.ValueYear).IsUnicode(false);
            });

            modelBuilder.Entity<FormsEntryDTO>().HasNoKey();

            modelBuilder.Entity<MailConfig>(entity =>
            {
                entity.Property(e => e.SmtpServer).IsUnicode(false);

                entity.Property(e => e.DefaultEmail).IsUnicode(false);

                entity.Property(e => e.SmtpPassword).IsUnicode(false);

                entity.Property(e => e.SmtpUsername).IsUnicode(false);
            });

            modelBuilder.Entity<MailMessage>(entity =>
            {
                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.MailFrom).IsUnicode(false);

                entity.Property(e => e.MailSubject).IsUnicode(false);

                entity.Property(e => e.MailType).IsUnicode(false);

                entity.Property(e => e.ReferenceNo).IsUnicode(false);
            });

            modelBuilder.Entity<MetaData>(entity =>
            {
                entity.Property(e => e.MetaDataType).IsUnicode(false);
            });

            modelBuilder.Entity<MetaDataRef>(entity =>
            {
                entity.Property(e => e.MetaDataType).IsUnicode(false);

                entity.Property(e => e.ReferenceId).IsUnicode(false);
            });

            modelBuilder.Entity<ReportOperatorEntry>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("ReportOperatorEntry");
            });

            modelBuilder.Entity<ReportsCategory>(entity =>
            {

                entity.Property(e => e.Title).IsUnicode(false);

                entity.Property(e => e.Description).IsUnicode(false);

            });

            modelBuilder.Entity<ReportTemplate>(entity =>
            {

                entity.Property(e => e.ReportId).IsUnicode(false);

                entity.Property(e => e.ReportType).IsUnicode(false);

                entity.Property(e => e.ReportName).IsUnicode(false);

                entity.Property(e => e.ReportSQL).IsUnicode(false);

                entity.Property(e => e.OrderBy).IsUnicode(false);

                entity.Property(e => e.FormFields).IsUnicode(false);

            });


            modelBuilder.Entity<Reports>(entity =>
            {

                entity.Property(e => e.Category).IsUnicode(false);

                entity.Property(e => e.ReportName).IsUnicode(false);

                entity.Property(e => e.ReportColumnName).IsUnicode(false);

                entity.Property(e => e.ReportColumnName).IsUnicode(false);

                entity.Property(e => e.ColumnColor).IsUnicode(false);

                entity.Property(e => e.ReportRowName).IsUnicode(false);

                entity.Property(e => e.ReportRowType).IsUnicode(false);

                entity.Property(e => e.ReportQuery).IsUnicode(false);

                entity.Property(e => e.ChartType).IsUnicode(false);

                entity.Property(e => e.CreatedBy).IsUnicode(false);

                entity.Property(e => e.AnalysisFields).IsUnicode(false);

                entity.Property(e => e.AnalysisAggregator).IsUnicode(false);

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateMigrated).HasDefaultValueSql("(getdate())");

            });

            modelBuilder.Entity<SsoInvoice>(entity =>
            {
                entity.Property(e => e.AppUserId).IsUnicode(false);

                entity.Property(e => e.ApplicationOrderId).IsUnicode(false);

                entity.Property(e => e.CalllbackUrl).IsUnicode(false);

                entity.Property(e => e.CompanyName).IsUnicode(false);

                entity.Property(e => e.CreatedBy).IsUnicode(false);

                entity.Property(e => e.CurrencyCode).IsUnicode(false);

                entity.Property(e => e.CurrencyId).IsUnicode(false);

                entity.Property(e => e.CurrencyName).IsUnicode(false);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.InvoiceNo).IsUnicode(false);

                entity.Property(e => e.OrderId).IsUnicode(false);

                entity.Property(e => e.PaymentBankBranch).IsUnicode(false);

                entity.Property(e => e.PaymentBankCountry).IsUnicode(false);

                entity.Property(e => e.PaymentBankName).IsUnicode(false);

                entity.Property(e => e.PaymentType).IsUnicode(false);

                entity.Property(e => e.PaymentUrl).IsUnicode(false);

                entity.Property(e => e.ReceiptNo).IsUnicode(false);

                entity.Property(e => e.ReceiptPath).IsUnicode(false);

                entity.Property(e => e.ReceiptUploadedBy).IsUnicode(false);

                entity.Property(e => e.ResponseCode).IsUnicode(false);

                entity.Property(e => e.Rrr).IsUnicode(false);

                entity.Property(e => e.Status).IsUnicode(false);

                entity.Property(e => e.TellerNo).IsUnicode(false);

                entity.Property(e => e.TransactionId).IsUnicode(false);
            });

            modelBuilder.Entity<SsoToken>(entity =>
            {
                entity.Property(e => e.DateLogin).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TokenSource).IsUnicode(false);

                entity.Property(e => e.TokenValue).IsUnicode(false);
            });

            modelBuilder.Entity<SsoUser>(entity =>
            {
                entity.Property(e => e.Active).IsUnicode(false);

                entity.Property(e => e.AllowedToUseApi).IsUnicode(false);

                entity.Property(e => e.AppUserEmail).IsUnicode(false);

                entity.Property(e => e.AppUserId).IsUnicode(false);

                entity.Property(e => e.DateLogin).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EmailVerified).IsUnicode(false);

                entity.Property(e => e.FirstName).IsUnicode(false);

                entity.Property(e => e.Image).IsUnicode(false);

                entity.Property(e => e.LastName).IsUnicode(false);

                entity.Property(e => e.LogoPath).IsUnicode(false);

                entity.Property(e => e.MiddleName).IsUnicode(false);

                entity.Property(e => e.MobileNumber).IsUnicode(false);

                entity.Property(e => e.OrganizationDescription).IsUnicode(false);

                entity.Property(e => e.OrganizationGroup).IsUnicode(false);

                entity.Property(e => e.OrganizationId).IsUnicode(false);

                entity.Property(e => e.OrganizationLongName).IsUnicode(false);

                entity.Property(e => e.OrganizationShortName).IsUnicode(false);

                entity.Property(e => e.PhoneVerified).IsUnicode(false);

                entity.Property(e => e.RoleName).IsUnicode(false);

                entity.Property(e => e.UserType).IsUnicode(false);

                entity.Property(e => e.Username).IsUnicode(false);
            });

            modelBuilder.Entity<SystemConfig>(entity =>
            {
                entity.Property(e => e.DocumentPath).IsUnicode(false);

                entity.Property(e => e.ImagePath).IsUnicode(false);

                entity.Property(e => e.SsoAppHost).IsUnicode(false);

                entity.Property(e => e.SsoAppHostProfile).IsUnicode(false);

                entity.Property(e => e.TaskClosureAccount).IsUnicode(false);
            });

            modelBuilder.Entity<TariffAnalysis>(entity =>
            {
                entity.Property(e => e.FieldId).IsUnicode(false);

                entity.Property(e => e.FieldName).IsUnicode(false);

                entity.Property(e => e.ParameterId).IsUnicode(false);
            });

            modelBuilder.Entity<TariffAnalysisReport>(entity =>
            {
                entity.Property(e => e.Amount).HasDefaultValueSql("((0))");

                entity.Property(e => e.BurnRate).HasDefaultValueSql("((0))");

                entity.Property(e => e.EffectiveTariff).HasDefaultValueSql("((0))");

                entity.Property(e => e.ParameterId).IsUnicode(false);
            });

            modelBuilder.Entity<TariffEvaluation>(entity =>
            {
                entity.Property(e => e.StartDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<TariffHistories>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("TariffHistories", "dbo");

                entity.Property(e => e.AppType).IsUnicode(false);

                entity.Property(e => e.FormName).IsUnicode(false);

                entity.Property(e => e.OperatorId).IsUnicode(false);
            });

            modelBuilder.Entity<UserPrivilege>(entity =>
            {
                entity.HasKey(e => e.RecId)
                    .HasName("PK_UserPrivilege_1");

                entity.Property(e => e.RoleId).IsUnicode(false);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(e => e.UserId).IsUnicode(false);

                entity.Property(e => e.AppUserId).IsUnicode(false);

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).IsUnicode(false);

                entity.Property(e => e.Fullname).IsUnicode(false);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.JobTitle).IsUnicode(false);

                entity.Property(e => e.OrganizationId).IsUnicode(false);

                entity.Property(e => e.Password).IsUnicode(false);

                entity.Property(e => e.RoleId).IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('UNDETERMINED')");

                entity.Property(e => e.Telephone).IsUnicode(false);

                entity.Property(e => e.UserType).IsUnicode(false);
            });
            


            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.Property(e => e.LoginDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LoginToken).IsUnicode(false);

                entity.Property(e => e.SessionVariable).IsUnicode(false);
            });

            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProcessId).IsUnicode(false);

                entity.Property(e => e.ProcessName).IsUnicode(false);

                entity.Property(e => e.Status).IsUnicode(false);

                OnModelCreatingPartial(modelBuilder);
            });

            modelBuilder.Entity<WorkflowActor>(entity =>
            {

                entity.Property(e => e.ProcessId).IsUnicode(false);

                entity.Property(e => e.ActorNumber).IsUnicode(false);

                entity.Property(e => e.ActorName).IsUnicode(false);

                OnModelCreatingPartial(modelBuilder);
            });
            
            modelBuilder.Entity<WorkflowLink>(entity =>
            {

                entity.Property(e => e.ProcessId).IsUnicode(false);

                entity.Property(e => e.LinkId).IsUnicode(false);

                entity.Property(e => e.FromActorNumber).IsUnicode(false);

                entity.Property(e => e.ToActorNumber).IsUnicode(false);

                OnModelCreatingPartial(modelBuilder);
            });

            modelBuilder.Entity<WorkflowDetails>(entity =>
            {
                entity.Property(e => e.ProcessId).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RoleId).IsUnicode(false);

                entity.Property(e => e.InputRoleId).IsUnicode(false);

                entity.Property(e => e.OutputRoleId).IsUnicode(false);

                OnModelCreatingPartial(modelBuilder);
            });

            modelBuilder.Entity<WorkflowManager>(entity =>
            {
                entity.Property(e => e.ActionId).IsUnicode(false);

                entity.Property(e => e.ActionUrl).IsUnicode(false);

                entity.Property(e => e.CompletionFlag)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('NO')");

                entity.Property(e => e.DateAssigned).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OperatorName).IsUnicode(false);

                entity.Property(e => e.ProcessId).IsUnicode(false);

                entity.Property(e => e.ReferenceNo).IsUnicode(false);

                entity.Property(e => e.RoleId).IsUnicode(false);

                entity.Property(e => e.TaskId).IsUnicode(false);

                entity.Property(e => e.TaskType)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('NEW')");

                entity.Property(e => e.UserId).IsUnicode(false);

                entity.Property(e => e.IsSource).IsUnicode(false);
            });

        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public DbSet<pcea.Models.Memo> Memo { get; set; }

    }
}
