using AppPrint_and_Wear.Models;
using Microsoft.EntityFrameworkCore;
using TuProyecto.Models;

namespace AppPrint_and_Wear.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Carrito_De_Compra> Carrito_De_Compras { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Metodo_De_Pago> Metodo_De_Pagos { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetalleFactura> DetalleFacturas { get; set; }
        public DbSet<Factura> Facturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Cliente 1 a muchos Métodos de pago
            modelBuilder.Entity<Cliente>()
                .HasMany(c => c.Metodo_De_Pagos)
                .WithOne(mp => mp.Cliente)
                .HasForeignKey(mp => mp.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Cliente 1 a 1 Carrito
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Carritos_De_Compras)
                .WithOne(ca => ca.Cliente)
                .HasForeignKey<Carrito_De_Compra>(ca => ca.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Carrito 1 a muchos CartItems
            modelBuilder.Entity<Carrito_De_Compra>()
                .HasMany(c => c.CartItems)
                .WithOne(i => i.Carrito_De_Compra)
                .HasForeignKey(i => i.Carrito_De_Compra_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Carrito a Método de pago (sin cascada)
            modelBuilder.Entity<Carrito_De_Compra>()
                .HasOne(c => c.Metodo_De_Pago)
                .WithMany()
                .HasForeignKey(c => c.Metodo_De_PagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Factura a Pedido (sin cascada para evitar múltiple paths)
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Pedido)
                .WithMany() // Cambia aquí si Pedido tiene colección de Facturas
                .HasForeignKey(f => f.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
