using System;
using System.Data;
using System.Data.SqlClient;

namespace ADO.NET_Individual
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Server=WIN-OBDH18C5VTL;Database=Experimental;Integrated Security=True;";
            using var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            // 1. знайти рік народження найстаршого юзера
            var selectTask1 = "select MIN(u.BirthYear) as TheOldestUser from Users u;";

            using var command = new SqlCommand(selectTask1, sqlConnection);

            var theOldestUser = command.ExecuteScalar();
            Console.WriteLine(theOldestUser);

            // 2. Додати нового юзера в базу витягнути айді доданого запису
            var selectTask2 = @"insert into Users values ('Bodya', 2002, 400); 
                                select top 1 u.Id from Users u order by u.Id desc;";

            using var getNewestIdCommand = new SqlCommand(selectTask2, sqlConnection);

            var IdOfNewestUser = getNewestIdCommand.ExecuteScalar();
            Console.WriteLine(IdOfNewestUser);

            // 3. робота з транзакціями
            // 4. Отримати результат зі сторед процедури через output parameter, вивести його на екран. Логіка процедури на ваш розсуд
            // дві айдішки юзерів передати як параметри. Всі гроші юзера1 перекинути на рахунок юзера2.Повернути суму перекидування

            var storedProcedureName = "sp_AccountTransfer";
            using var transaction = sqlConnection.BeginTransaction();
            using var transactionCommand = new SqlCommand(storedProcedureName, sqlConnection, transaction);
            int firstUserId = 2;
            int secondUserId = 3;
            double accountTransactionResult = 0;

            try
            {
                transactionCommand.CommandType = CommandType.StoredProcedure;
                transactionCommand.Parameters.AddWithValue("@firstUserId", firstUserId);
                transactionCommand.Parameters.AddWithValue("@secondUserId", secondUserId);
                var output = transactionCommand.Parameters.Add("@result", SqlDbType.Float);
                output.Direction = ParameterDirection.Output;

                transactionCommand.ExecuteNonQuery();
                accountTransactionResult = (double)output.Value;

                transaction.Commit();

                Console.WriteLine(accountTransactionResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transaction.Rollback();
            }
        }
    }
}
