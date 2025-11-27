using System;
using System.Collections.Generic;
using Lunchbox.Models;
using Microsoft.EntityFrameworkCore;

namespace Lunchbox.Data;

public partial class LunchboxContext : DbContext
{
    public LunchboxContext()
    {
    }

    public LunchboxContext(DbContextOptions<LunchboxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Allergy> Allergies { get; set; }

    public virtual DbSet<Child> Children { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<MealPackage> MealPackages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Parent> Parents { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PreMadeMeal> PreMadeMeals { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<SavedMeal> SavedMeals { get; set; }

    public virtual DbSet<User> Users { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__719FE4E8D588B976");

            entity.ToTable("Admin");

            entity.HasIndex(e => e.Email, "UQ__Admin__A9D10534B346DBEF").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Admins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Admin__UserID__71D1E811");
        });

        modelBuilder.Entity<Allergy>(entity =>
        {
            entity.HasKey(e => e.AllergyId).HasName("PK__Allergy__A49EBE62D1E8BE56");

            entity.ToTable("Allergy");

            entity.Property(e => e.AllergyId).HasColumnName("AllergyID");
            entity.Property(e => e.AllergyType).HasMaxLength(100);
        });

        modelBuilder.Entity<Child>(entity =>
        {
            entity.HasKey(e => e.ChildId).HasName("PK__Child__BEFA0736DF49D2D5");

            entity.ToTable("Child");

            entity.Property(e => e.ChildId).HasColumnName("ChildID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Child__ParentID__72C60C4A");

            entity.HasMany(d => d.Allergies).WithMany(p => p.Children)
                .UsingEntity<Dictionary<string, object>>(
                    "ChildAllergy",
                    r => r.HasOne<Allergy>().WithMany()
                        .HasForeignKey("AllergyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Child_All__Aller__73BA3083"),
                    l => l.HasOne<Child>().WithMany()
                        .HasForeignKey("ChildId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Child_All__Child__74AE54BC"),
                    j =>
                    {
                        j.HasKey("ChildId", "AllergyId").HasName("PK__Child_Al__84B3ECD0C9E24880");
                        j.ToTable("Child_Allergy");
                        j.IndexerProperty<int>("ChildId").HasColumnName("ChildID");
                        j.IndexerProperty<int>("AllergyId").HasColumnName("AllergyID");
                    });
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Item__727E83EBEE391C64");

            entity.ToTable("Item");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.CarbsG)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("Carbs_g");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.FatG)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("Fat_g");
            entity.Property(e => e.ItemCategory).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ProteinG)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("Protein_g");
            entity.Property(e => e.SodiumMg)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("Sodium_mg");
            entity.Property(e => e.SugarG)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("Sugar_g");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasMany(d => d.Allergies).WithMany(p => p.Items)
                .UsingEntity<Dictionary<string, object>>(
                    "ItemAllergy",
                    r => r.HasOne<Allergy>().WithMany()
                        .HasForeignKey("AllergyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Item_Alle__Aller__75A278F5"),
                    l => l.HasOne<Item>().WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Item_Alle__ItemI__76969D2E"),
                    j =>
                    {
                        j.HasKey("ItemId", "AllergyId").HasName("PK__Item_All__4837680DC0AB61C1");
                        j.ToTable("Item_Allergy");
                        j.IndexerProperty<int>("ItemId").HasColumnName("ItemID");
                        j.IndexerProperty<int>("AllergyId").HasColumnName("AllergyID");
                    });
        });

        modelBuilder.Entity<MealPackage>(entity =>
        {
            entity.HasKey(e => e.MealPackageId).HasName("PK__MealPack__9CA057C3872DA2D6");

            entity.ToTable("MealPackage");

            entity.Property(e => e.MealPackageId).HasColumnName("MealPackageID");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PackageDescription).HasMaxLength(255);
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAF78FB833F");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ChildId).HasColumnName("ChildID");
            entity.Property(e => e.DeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.DeliveryStatus).HasMaxLength(50);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Child).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__ChildID__778AC167");

            entity.HasOne(d => d.Package).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__Order__PackageID__787EE5A0");

            entity.HasOne(d => d.Parent).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__ParentID__797309D9");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__Order_It__57ED06A1FB543355");

            entity.ToTable("Order_Item");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PreMadeMealId).HasColumnName("PreMadeMealID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Item).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Order_Ite__ItemI__7A672E12");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order_Ite__Order__7B5B524B");

            entity.HasOne(d => d.PreMadeMeal).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.PreMadeMealId)
                .HasConstraintName("FK__Order_Ite__PreMa__7C4F7684");
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.ParentId).HasName("PK__Parent__D339510F33D99B64");

            entity.ToTable("Parent");

            entity.HasIndex(e => e.Email, "UQ__Parent__A9D10534C610E62B").IsUnique();

            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Parents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Parent__UserID__7D439ABD");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A58F6A22294");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__OrderID__7E37BEF6");
        });

        modelBuilder.Entity<PreMadeMeal>(entity =>
        {
            entity.HasKey(e => e.PreMadeMealId).HasName("PK__PreMadeM__421FE174D55D63BD");

            entity.ToTable("PreMadeMeal");

            entity.Property(e => e.PreMadeMealId).HasColumnName("PreMadeMealID");
            entity.Property(e => e.FixedPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.MealDescription).HasMaxLength(255);
            entity.Property(e => e.MealName).HasMaxLength(100);

            entity.HasMany(d => d.Items).WithMany(p => p.PreMadeMeals)
                .UsingEntity<Dictionary<string, object>>(
                    "PreMadeMealItem",
                    r => r.HasOne<Item>().WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PreMadeMe__ItemI__7F2BE32F"),
                    l => l.HasOne<PreMadeMeal>().WithMany()
                        .HasForeignKey("PreMadeMealId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PreMadeMe__PreMa__00200768"),
                    j =>
                    {
                        j.HasKey("PreMadeMealId", "ItemId").HasName("PK__PreMadeM__E538094A02FD0D51");
                        j.ToTable("PreMadeMeal_Item");
                        j.IndexerProperty<int>("PreMadeMealId").HasColumnName("PreMadeMealID");
                        j.IndexerProperty<int>("ItemId").HasColumnName("ItemID");
                    });
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Rating__FCCDF85CD31E6D4B");

            entity.ToTable("Rating");

            entity.Property(e => e.RatingId).HasColumnName("RatingID");
            entity.Property(e => e.Comment).HasMaxLength(255);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.RatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rating__OrderID__01142BA1");
        });

        modelBuilder.Entity<SavedMeal>(entity =>
        {
            entity.HasKey(e => e.SavedMealId).HasName("PK__SavedMea__D4822EF60DCA67CC");

            entity.ToTable("SavedMeal");

            entity.Property(e => e.SavedMealId).HasColumnName("SavedMealID");
            entity.Property(e => e.ChildId).HasColumnName("ChildID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");

            entity.HasOne(d => d.Child).WithMany(p => p.SavedMeals)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedMeal__Child__02084FDA");

            entity.HasOne(d => d.Parent).WithMany(p => p.SavedMeals)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SavedMeal__Paren__02FC7413");

            entity.HasMany(d => d.Items).WithMany(p => p.SavedMeals)
                .UsingEntity<Dictionary<string, object>>(
                    "SavedMealItem",
                    r => r.HasOne<Item>().WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SavedMeal__ItemI__03F0984C"),
                    l => l.HasOne<SavedMeal>().WithMany()
                        .HasForeignKey("SavedMealId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SavedMeal__Saved__04E4BC85"),
                    j =>
                    {
                        j.HasKey("SavedMealId", "ItemId").HasName("PK__SavedMea__73A5C6C825F8FC80");
                        j.ToTable("SavedMeal_Item");
                        j.IndexerProperty<int>("SavedMealId").HasColumnName("SavedMealID");
                        j.IndexerProperty<int>("ItemId").HasColumnName("ItemID");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC381A4536");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D105345B40107D").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
