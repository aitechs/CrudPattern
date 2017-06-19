using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using AiTech.CrudPattern;

namespace Accounts.Model
{
    [Table("AccountToken")]
    public class AccountToken: Entity
    {		
		Dictionary<string,object> OriginalValues;

		#region Default Properties
		        		
        public string Username {get; set;}        		
        public string Token {get; set;}        		
        public DateTime Expiration {get; set;}	
		
		#endregion
	
		protected void InitializeTrackingChanges()
		{
			OriginalValues = new Dictionary<string,object>();
			        
		 	OriginalValues.Add("Username", this.Username);        
		 	OriginalValues.Add("Token", this.Token);        
		 	OriginalValues.Add("Expiration", this.Expiration);		
		}

		protected Dictionary<string,object> GetChanges()
		{
			var changes = new Dictionary<string, object>();

			        		 	
			if(!Equals(this.Username, OriginalValues["Username"]))
				changes.Add("Username", this.Username);
			        		 	
			if(!Equals(this.Token, OriginalValues["Token"]))
				changes.Add("Token", this.Token);
			        		 	
			if(!Equals(this.Expiration, OriginalValues["Expiration"]))
				changes.Add("Expiration", this.Expiration);
			            
			
            return changes;
		}

		
	}


	
}
