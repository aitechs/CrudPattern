using Dapper;
using System.Collections.Generic;


namespace AiTech.LiteOrm.Database.Search
{
    public static class Search
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchString"></param>
        /// <param name="query">Must have @Criteria string</param>
        /// <param name="searchStyle"></param>
        /// <returns></returns>
        public static IEnumerable<T> SearchData<T>(string searchString, string query, SearchStyleEnum searchStyle)
        {
            using (var db = Database.Connection.CreateConnection())
            {
                db.Open();

                var criteria = "";
                switch (searchStyle)
                {
                    case SearchStyleEnum.StartsWith: criteria = searchString + "%"; break;
                    case SearchStyleEnum.Contains: criteria = "%" + searchString + "%"; break;
                    case SearchStyleEnum.EndsWith: criteria = "%" + searchString; break;
                }

                return db.Query<T>(query, new { Criteria = criteria });
            }
        }

    }
}