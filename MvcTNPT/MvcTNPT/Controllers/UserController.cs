using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Web.Configuration;

using MongoDB.Driver;
using MongoDB.Bson;
using MvcTNPT.Models;
using MongoDB.Driver.Builders;

namespace MvcTNPT.Controllers
{
    public class UserController : Controller
    {
        int awardsAdded = 0;
        int contribsAdded = 0;
        int developersAdded = 0;
        public ActionResult Index()
        {
            // Get all users by sorting by createdate
            var users = MongoWrapper.GetDatabase().
                            GetCollection("tnpt").
                            FindAll();
            foreach (var user in users)
            {
                WriteRecord(user);
            }

            return View(users);
        }
        public int AddAwards(SqlConnection sqlConnection, BsonDocument user)
        {
            sqlConnection.Open();
            int awards=0;
            bool AwardExsists = user.Names.Contains("awards");
            MongoDB.Bson.BsonArray award = new BsonArray();
            if (AwardExsists)
            {
                award = user["awards"].AsBsonArray;
            }
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlCommand recordExsistsAwards = new System.Data.SqlClient.SqlCommand();

            cmd.Connection = sqlConnection;
            recordExsistsAwards.Connection = sqlConnection;
            cmd.CommandType = System.Data.CommandType.Text;

            double year=0.0;
            for (int i = 0; i < award.Count; i++)
            {
                cmd.CommandText = "sp_Addawards";
                cmd.CommandType = CommandType.StoredProcedure;

                //cmd.CommandText = "insert into awards(first_name, last_name,birth,award,year,[by]) values (@firstname,@lastname,@birth,@award,@year,@by)";
                cmd.Parameters.AddWithValue("@firstname", (user["name"]["first"].AsString));
                cmd.Parameters.AddWithValue("@lastname", (user["name"]["last"].AsString));
                cmd.Parameters.AddWithValue("@birth", (user["birth"].AsDateTime));
                if (AwardExsists)
                {
                    cmd.Parameters.AddWithValue("@award", (award[i]["award"].AsString));
                    year = 0;
                    if ((award[i]["year"].GetType() == typeof(string)))
                    {
                        year = Convert.ToDouble(award[i]["year"]);
                    }
                    else
                    {
                        year = award[i]["year"].ToDouble();
                    }
                    cmd.Parameters.AddWithValue("@year", year);
                    cmd.Parameters.AddWithValue("@by", (award[i]["by"].AsString));
                }

                recordExsistsAwards.CommandText = "select count(*) from awards where first_name='" + user["name"]["first"].AsString + "' and last_name='" + user["name"]["last"].AsString + "' and birth='" + user["birth"].AsDateTime + "' and award='" + award[i]["award"].AsString + "' and year='" + year + "' and [by]='" + award[i]["by"].AsString + "'";
                int count = (int)recordExsistsAwards.ExecuteScalar();
               if (Convert.ToInt32(count) == 0)
                {
                    awards++;
                    cmd.ExecuteNonQuery();
                }
                cmd.Parameters.Clear();
            }

            sqlConnection.Close();
            return (awards);


        }
        public int AddContribs(SqlConnection sqlConnection, BsonDocument user)
        {
            int contribscount = 0;
            sqlConnection.Open();
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlCommand recordExsists = new System.Data.SqlClient.SqlCommand();
            cmd.Connection = sqlConnection;
            recordExsists.Connection = sqlConnection;
            cmd.CommandType = System.Data.CommandType.Text;

            bool ContribeExsists = user.Names.Contains("contribs");

            MongoDB.Bson.BsonArray contribe = new BsonArray();
            if (ContribeExsists)
            {
                contribe = user["contribs"].AsBsonArray;
            }
            for (int i = 0; i < contribe.Count; i++)
            {
                cmd.CommandText = "sp_Addcontribs";
                cmd.CommandType = CommandType.StoredProcedure;

                //cmd.CommandText = "insert into contribs(first_name, last_name,birth,contribs) values (@firstname,@lastname,@birth,@contribe)";
                cmd.Parameters.AddWithValue("@firstname", (user["name"]["first"].AsString));
                cmd.Parameters.AddWithValue("@lastname", (user["name"]["last"].AsString));
                cmd.Parameters.AddWithValue("@birth", (user["birth"].AsDateTime));
                if (ContribeExsists)
                {
                    cmd.Parameters.AddWithValue("@contribe", (contribe[i].AsString));
                }
                //instead of stored procedure I decided to use sql commands in here just to show different way it can be done.
                recordExsists.CommandText = "select count(*) from contribs where first_name='" + user["name"]["first"].AsString + "' and last_name='" + user["name"]["last"].AsString + "' and birth='" + user["birth"].AsDateTime + "' and contribs='" + contribe[i].AsString + "'";

                int count = (int)recordExsists.ExecuteScalar();
                if (count == 0)
                {
                    contribscount++;
                    cmd.ExecuteNonQuery();
                }
                cmd.Parameters.Clear();
            }

            ////insert records in developer
            sqlConnection.Close();
            return (contribscount);
        }

        public int AddDevelopres(SqlConnection sqlConnection, BsonDocument user)
        {
            int developerCount = 0;
            sqlConnection.Open();
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlCommand recordExsists = new System.Data.SqlClient.SqlCommand();
            cmd.Connection = sqlConnection;
            recordExsists.Connection = sqlConnection;
            cmd.CommandText = "sp_Adddeveloper";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@firstname", (user["name"]["first"].AsString));
            cmd.Parameters.AddWithValue("@lastname", (user["name"]["last"].AsString));
            if (user.Names.Contains("title"))
            {
                cmd.Parameters.AddWithValue("@title", (user["title"].AsString));
            }
            else
            {
                cmd.Parameters.AddWithValue("@title", "");

            }
            DateTime? death = null;
            if (user.Names.Contains("death"))
            {
                death = (user["death"].AsDateTime);
            }
            cmd.Parameters.AddWithValue("@death", death);
            cmd.Parameters.AddWithValue("@birth", (user["birth"].AsDateTime));
                recordExsists.CommandText = "select count(*) from developers where first_name='" + user["name"]["first"].AsString + "' and last_name='" + user["name"]["last"].AsString + "' and birth='" + user["birth"].AsDateTime + "'";
                int count = (int)recordExsists.ExecuteScalar();
                if (count == 0)
                {
                    cmd.ExecuteNonQuery();
                    developerCount++;
                }
            cmd.Parameters.Clear();
            
            sqlConnection.Close();
            return (developerCount);

        }


        public void WriteRecord(BsonDocument user)
        {
            System.Data.SqlClient.SqlConnection sqlConnection1 =
    new System.Data.SqlClient.SqlConnection(WebConfigurationManager.ConnectionStrings["TNPT"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            cmd.Connection = sqlConnection1;

            int awardsAddedperCollection=AddAwards(sqlConnection1, user);
            awardsAdded = awardsAdded + awardsAddedperCollection;
            ViewBag.totalawardsrecord = "Total number of record(s) added in Awards table " + awardsAdded;
            //AddContribs(sqlConnection1, user);
            int contribsAddedperCollection = AddContribs(sqlConnection1, user);
            contribsAdded = contribsAdded + contribsAddedperCollection;
            ViewBag.totalcontribsrecord = "Total number of record(s) added in Contribs table " + contribsAdded;

            int developersAddedperCollection = AddDevelopres(sqlConnection1, user);
            developersAdded = developersAdded + developersAddedperCollection;
            ViewBag.totaldeveloperrecord = "Total number of record(s) added in Developer table " + developersAdded;

        }
    }
}

