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
    /// <summary>
    /// This controller get all the documents from Mongodb and put them in sql database after processing them.
    /// </summary>
    public class UserController : Controller
    {
        // Following variables keep track of how many new records got added.
        int awardsAdded = 0;
        int contribsAdded = 0;
        int developersAdded = 0;
        public ActionResult Index()
        {
            // Get all users 
            var users = MongoWrapper.GetDatabase().
                            GetCollection("tnpt").
                            FindAll();
            //now that we got all the documents lets process them one by one.
            long total_documents=users.Count();
            ViewBag.totalrecords = "Total number of document read " + total_documents;

            foreach (var user in users)
            {
                WriteRecord(user);
            }

            return View(users);
        }
        public void WriteRecord(BsonDocument user)
        {
            System.Data.SqlClient.SqlConnection sqlConnection1 =
    new System.Data.SqlClient.SqlConnection(WebConfigurationManager.ConnectionStrings["TNPT"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            cmd.Connection = sqlConnection1;
            //Process all the documents that are related to Awards table and then generate the number indicating the number of total rows added
            int awardsAddedperCollection = AddAwards(sqlConnection1, user);
            awardsAdded = awardsAdded + awardsAddedperCollection;
            ViewBag.totalawardsrecord = "Total number of record(s) added in Awards table " + awardsAdded;
            //Process all the documents that are related to Contribs table and then generate the number indicating the number of total rows added
            int contribsAddedperCollection = AddContribs(sqlConnection1, user);
            contribsAdded = contribsAdded + contribsAddedperCollection;
            ViewBag.totalcontribsrecord = "Total number of record(s) added in Contribs table " + contribsAdded;
            //Process all the documents that are related to Developer table and then generate the number indicating the number of total rows added
            int developersAddedperCollection = AddDevelopres(sqlConnection1, user);
            developersAdded = developersAdded + developersAddedperCollection;
            ViewBag.totaldeveloperrecord = "Total number of record(s) added in Developer table " + developersAdded;

        }
        /// <summary>
        /// Following method process all the documents related to Awards table.
        /// </summary>
        /// <param name="sqlConnection"> SQL Connection</param>
        /// <param name="user">Document from collections</param>
        /// <returns></returns>
        public int AddAwards(SqlConnection sqlConnection, BsonDocument user)
        {
            sqlConnection.Open();
            int awards=0;
            // Chceck to see if this document has items related to Awards.
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
                //After we have found the document related to Awards lets just add them into the SQL table through stored procedure.
                cmd.CommandText = "sp_Addawards";
                cmd.CommandType = CommandType.StoredProcedure;

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
                //Because we have to only insert records that does not exsist in the sql database we have to check to see if they are new or not.
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
       /// <summary>
       /// This Method adds Contribs into the related table in SQL.
       /// </summary>
       /// <param name="sqlConnection"></param>
       /// <param name="user"></param>
       /// <returns></returns>
        public int AddContribs(SqlConnection sqlConnection, BsonDocument user)
        {
            int contribscount = 0;
            sqlConnection.Open();
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            System.Data.SqlClient.SqlCommand recordExsists = new System.Data.SqlClient.SqlCommand();
            cmd.Connection = sqlConnection;
            recordExsists.Connection = sqlConnection;
            cmd.CommandType = System.Data.CommandType.Text;
            // Check to see if the records are related to contribs.
            bool ContribeExsists = user.Names.Contains("contribs");

            MongoDB.Bson.BsonArray contribe = new BsonArray();
            if (ContribeExsists)
            {
                contribe = user["contribs"].AsBsonArray;
            }
            // Now lets add them in SQL table one by one.
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
                // Because only new records have to be added lets check and see if it i in fact new record.
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

            sqlConnection.Close();
            return (contribscount);
        }
        /// <summary>
        /// This method adds all the document related to Developers into SQL Table.
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public int AddDevelopres(SqlConnection sqlConnection, BsonDocument user)
        {
            //This methos is pretty straight forward we ust have to add all the document into the SQL Table.
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
    }
}

