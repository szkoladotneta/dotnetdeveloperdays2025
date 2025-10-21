using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class OrderService
{
    private readonly IConfiguration _configuration;

    public OrderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<Order> GetUserOrders(int userId)
    {
        var orders = new List<Order>();
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Get all orders - PROBLEM: No pagination
            var query = "SELECT * FROM Orders WHERE UserId = " + userId;
            var command = new SqlCommand(query, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var order = new Order
                {
                    Id = (int)reader["Id"],
                    UserId = (int)reader["UserId"],
                    TotalAmount = (decimal)reader["TotalAmount"],
                    Status = (string)reader["Status"],
                    CreatedAt = (DateTime)reader["CreatedAt"]
                };

                // PROBLEM: N+1 query problem!
                order.Items = GetOrderItems(order.Id);

                orders.Add(order);
            }
        }

        return orders;
    }

    private List<OrderItem> GetOrderItems(int orderId)
    {
        var items = new List<OrderItem>();
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            var query = "SELECT * FROM OrderItems WHERE OrderId = " + orderId;
            var command = new SqlCommand(query, connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new OrderItem
                {
                    Id = (int)reader["Id"],
                    OrderId = (int)reader["OrderId"],
                    ProductId = (int)reader["ProductId"],
                    Quantity = (int)reader["Quantity"],
                    Price = (decimal)reader["Price"]
                });
            }
        }

        return items;
    }

    public void CreateOrder(Order order)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // PROBLEM: No transaction! If second insert fails, data inconsistency
            var query = $"INSERT INTO Orders (UserId, TotalAmount, Status, CreatedAt) " +
                       $"VALUES ({order.UserId}, {order.TotalAmount}, '{order.Status}', GETDATE())";

            var command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();

            // PROBLEM: How do we get the new order ID? This won't work.
            foreach (var item in order.Items)
            {
                var itemQuery = $"INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price) " +
                               $"VALUES (0, {item.ProductId}, {item.Quantity}, {item.Price})";
                var itemCommand = new SqlCommand(itemQuery, connection);
                itemCommand.ExecuteNonQuery();
            }
        }
    }
}