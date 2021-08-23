using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace Lecture6
{
    class Program
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
        static SqlDataAdapter moviesAdapter;
        static DataTable moviesTable;
        static void Main(string[] args)
        {
            //Creating 2 tables
            //Movies
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                var sqlQuery = "CREATE TABLE Movies(Id int not null identity(1,1) primary key, Title NVARCHAR(255), Director NVARCHAR(255));";
                using (var sqlCommand = new SqlCommand(sqlQuery, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
            //Actors
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                var sqlQuery = "CREATE TABLE Actors(Id int not null identity(1,1) primary key, FirstName NVARCHAR(255), LastName NVARCHAR(255));";
                using (var sqlCommand = new SqlCommand(sqlQuery, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }

            //Data manipulations with DATA TABLE and DATA ADAPTER
            moviesTable = CreateMoviesTable();

            moviesAdapter = CreateMoviesAdapter();

            moviesAdapter.Fill(moviesTable);
            PrintMovies();

            Console.WriteLine();

            UpdateMovie(new Movie("The Green Knight", "David Lowery"), 1);
            DeleteMovie(2);
            InsertMovies();

            moviesTable.Clear();
            moviesAdapter.Fill(moviesTable);
            PrintMovies();
        }

        public static DataTable CreateMoviesTable()
        {
            DataTable moviesTableTmp = new DataTable("Movies");
            moviesTableTmp.Columns.Add("Id", typeof(int));
            moviesTableTmp.Columns.Add("Title", typeof(string));
            moviesTableTmp.Columns.Add("Director", typeof(string));
            return moviesTableTmp;
        }

        public static SqlDataAdapter CreateMoviesAdapter()
        {
            //SELECT
            var moviesAdapterTmp = new SqlDataAdapter("SELECT * FROM Movies", connectionString);

            //INSERT
            moviesAdapterTmp.InsertCommand = new SqlCommand("INSERT INTO Movies(Title, Director)" +
                " VALUES(@Title, @Director)");
            moviesAdapterTmp.InsertCommand.Parameters.Add("@Title", SqlDbType.VarChar, 255, "Title");
            moviesAdapterTmp.InsertCommand.Parameters.Add("@Director", SqlDbType.VarChar, 255, "Director");

            //UPDATE
            moviesAdapterTmp.UpdateCommand = new SqlCommand("UPDATE Movies SET Title = @Title, " +
                "Director = @Director WHERE Id = @Id");
            moviesAdapterTmp.UpdateCommand.Parameters.Add("@Title", SqlDbType.VarChar, 255, "Title");
            moviesAdapterTmp.UpdateCommand.Parameters.Add("@Director", SqlDbType.VarChar, 255, "Director");
            SqlParameter updateParameter = moviesAdapterTmp.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int);
            updateParameter.SourceColumn = "Id";
            updateParameter.SourceVersion = DataRowVersion.Original;

            //DELETE
            moviesAdapterTmp.DeleteCommand = new SqlCommand("DELETE FROM Movies WHERE Id = @Id");
            SqlParameter deleteParameter = moviesAdapterTmp.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int);
            deleteParameter.SourceColumn = "Id";
            deleteParameter.SourceVersion = DataRowVersion.Original;

            return moviesAdapterTmp;
        }

        public static void InsertMovies()
        {
            string[,] movieInformation = { { "Druk", "Thomas Vinterberg" },
                                           { "Eternals", "Chloe Zhao" },
                                           { "Soul", "Pete Docter" } };

            for (int i = 0; i < movieInformation.GetLength(0); i++)
            {
                DataRow row = moviesTable.NewRow();
                row["Title"] = movieInformation[i, 0];
                row["Director"] = movieInformation[i, 1];
                moviesTable.Rows.Add(row);
            }

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                moviesAdapter.InsertCommand.Connection = sqlConnection;
                moviesAdapter.Update(moviesTable);
            }
        }

        public static void UpdateMovie(Movie movie, int id)
        {
            foreach (DataRow row in moviesTable.Rows)
            {
                if ((int)row["Id"] == id)
                {
                    row["Title"] = movie.Title;
                    row["Director"] = movie.Director;
                }
            }

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                moviesAdapter.UpdateCommand.Connection = sqlConnection;
                moviesAdapter.Update(moviesTable);
            }
        }

        public static void DeleteMovie(int id) 
        {
            foreach (DataRow row in moviesTable.Rows)
            {
                if ((int)row["Id"] == id)
                {
                    row.Delete();
                }
            }

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                moviesAdapter.DeleteCommand.Connection = sqlConnection;
                moviesAdapter.Update(moviesTable);
            }
        }
        public static void PrintMovies()
        {
            foreach (DataRow row in moviesTable.Rows)
            {
                Console.WriteLine($"Id = {row["Id"]}, Title = {row["Title"]}, Director = {row["Director"]}");
            }
        }
    }

    class Movie
    {
        public Movie()
        {

        }

        public Movie(string title, string director)
        {
            Title = title;
            Director = director;
        }
        public string Title { get; set; }

        public string Director { get; set; }
    }
}
