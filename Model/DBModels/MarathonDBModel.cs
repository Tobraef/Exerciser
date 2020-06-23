using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace App1.Model.DBModels
{
    [Table(TableName)]
    public class MarathonDBModel
    {
        public const string TableName = "Marathon";

        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int Distance { get; set; }
        /// <summary>
        /// At what difference between scheduled speed and current should pop (range 0% - 100%)
        /// </summary>
        public double DifferenceNotify { get; set; }
    }
}
