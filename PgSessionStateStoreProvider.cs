//
// $Id$
//
// Copyright © 2007 - 2008 Nauck IT KG		http://www.nauck-it.de
//
// Author:
//	Daniel Nauck		<d.nauck(at)nauck-it.de>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using Npgsql;
using NpgsqlTypes;

namespace NauckIT.PostgreSQLProvider
{
	public class PgSessionStateStoreProvider : SessionStateStoreProviderBase
	{
		private const string s_tableName = "Sessions";
		private string m_connectionString = string.Empty;
		private string m_applicationName = string.Empty;
		private SessionStateSection m_config = null;

		/// <summary>
		/// System.Configuration.Provider.ProviderBase.Initialize Method
		/// </summary>
		public override void Initialize(string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("Config", Properties.Resources.ErrArgumentNull);

			if (string.IsNullOrEmpty(name))
				name = Properties.Resources.SessionStoreProviderDefaultName;

			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", Properties.Resources.SessionStoreProviderDefaultDescription);
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			m_applicationName = PgMembershipProvider.GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

			// Get connection string.
			m_connectionString = PgMembershipProvider.GetConnectionString(config["connectionStringName"]);

			// Get <sessionState> configuration element.
			m_config = (SessionStateSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath).GetSection("system.web/sessionState");
		}

		/// <summary>
		/// SessionStateStoreProviderBase members
		/// </summary>
		#region SessionStateStoreProviderBase members

		public override void Dispose()
		{
		}

		/// <summary>
		/// SessionStateProviderBase.InitializeRequest
		/// </summary>
		public override void InitializeRequest(HttpContext context)
		{
		}

		/// <summary>
		/// SessionStateProviderBase.EndRequest
		/// </summary>
		public override void EndRequest(HttpContext context)
		{
		}

		/// <summary>
		/// SessionStateProviderBase.CreateNewStoreData
		/// </summary>
		public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
		{
			return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		/// <summary>
		/// SessionStateProviderBase.CreateUninitializedItem
		/// </summary>
		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
			{
				using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"SessionId\", \"ApplicationName\", \"Created\", \"Expires\", \"Timeout\", \"Locked\", \"LockId\", \"LockDate\", \"Data\", \"Flags\") Values (@SessionId, @ApplicationName, @Created, @Expires, @Timeout, @Locked, @LockId, @LockDate, @Data, @Flags)", s_tableName);

					dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
					dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;
					dbCommand.Parameters.Add("@Created", NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
					dbCommand.Parameters.Add("@Expires", NpgsqlDbType.TimestampTZ).Value = DateTime.Now.AddMinutes((Double)timeout);
					dbCommand.Parameters.Add("@Timeout", NpgsqlDbType.Integer).Value = timeout;
					dbCommand.Parameters.Add("@Locked", NpgsqlDbType.Boolean).Value = false;
					dbCommand.Parameters.Add("@LockId", NpgsqlDbType.Integer).Value = 0;
					dbCommand.Parameters.Add("@LockDate", NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
					dbCommand.Parameters.Add("@Data", NpgsqlDbType.Text).Value = string.Empty;
					dbCommand.Parameters.Add("@Flags", NpgsqlDbType.Integer).Value = 1;

					NpgsqlTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						dbTrans = dbConn.BeginTransaction();

						dbCommand.ExecuteNonQuery();

						// Attempt to commit the transaction
						dbTrans.Commit();
					}
					catch (Exception e)
					{
						Trace.WriteLine(e.ToString());

						if (dbTrans != null)
						{
							try
							{
								// Attempt to roll back the transaction
								Trace.WriteLine(Properties.Resources.LogRollbackAttempt);
								dbTrans.Rollback();
							}
							catch (NpgsqlException re)
							{
								// Rollback failed
								Trace.WriteLine(Properties.Resources.ErrRollbackFailed);
								Trace.WriteLine(re.ToString());
							}
						}

						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbTrans != null)
							dbTrans.Dispose();

						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}

		/// <summary>
		/// SessionStateProviderBase.GetItem
		/// </summary>
		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actions);
		}

		/// <summary>
		/// SessionStateProviderBase.GetItemExclusive
		/// </summary>
		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
		}

		/// <summary>
		/// SessionStateProviderBase.ReleaseItemExclusive
		/// </summary>
		public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
			{
				using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE \"{0}\" SET \"Expires\" = @Expires, \"Locked\" = @Locked WHERE \"SessionId\" = @SessionId AND \"ApplicationName\" = @ApplicationName AND \"LockId\" = @LockId", s_tableName);

					dbCommand.Parameters.Add("@Expires", NpgsqlDbType.TimestampTZ).Value = DateTime.Now.AddMinutes(m_config.Timeout.Minutes);
					dbCommand.Parameters.Add("@Locked", NpgsqlDbType.Boolean).Value = false;
					dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
					dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;
					dbCommand.Parameters.Add("@LockId", NpgsqlDbType.Integer).Value = lockId;
					
					NpgsqlTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						dbTrans = dbConn.BeginTransaction();

						dbCommand.ExecuteNonQuery();

						// Attempt to commit the transaction
						dbTrans.Commit();
					}
					catch (NpgsqlException e)
					{
						Trace.WriteLine(e.ToString());

						if (dbTrans != null)
						{
							try
							{
								// Attempt to roll back the transaction
								Trace.WriteLine(Properties.Resources.LogRollbackAttempt);
								dbTrans.Rollback();
							}
							catch (NpgsqlException re)
							{
								// Rollback failed
								Trace.WriteLine(Properties.Resources.ErrRollbackFailed);
								Trace.WriteLine(re.ToString());
							}
						}

						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbTrans != null)
							dbTrans.Dispose();

						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}

		/// <summary>
		/// SessionStateProviderBase.RemoveItem
		/// </summary>
		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
			{
				using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM \"{0}\" WHERE \"SessionId\" = @SessionId AND \"ApplicationName\" = @ApplicationName AND \"LockId\" = @LockId", s_tableName);

					dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
					dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;
					dbCommand.Parameters.Add("@LockId", NpgsqlDbType.Integer).Value = lockId;

					NpgsqlTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						dbTrans = dbConn.BeginTransaction();

						dbCommand.ExecuteNonQuery();

						// Attempt to commit the transaction
						dbTrans.Commit();
					}
					catch (Exception e)
					{
						Trace.WriteLine(e.ToString());

						if (dbTrans != null)
						{
							try
							{
								// Attempt to roll back the transaction
								Trace.WriteLine(Properties.Resources.LogRollbackAttempt);
								dbTrans.Rollback();
							}
							catch (Exception re)
							{
								// Rollback failed
								Trace.WriteLine(Properties.Resources.ErrRollbackFailed);
								Trace.WriteLine(re.ToString());
							}
						}

						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbTrans != null)
							dbTrans.Dispose();

						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}

		/// <summary>
		/// SessionStateProviderBase.ResetItemTimeout
		/// </summary>
		public override void ResetItemTimeout(HttpContext context, string id)
		{
			using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
			{
				using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
				{
					dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE \"{0}\" SET \"Expires\" = @Expires WHERE \"SessionId\" = @SessionId AND \"ApplicationName\" = @ApplicationName", s_tableName);

					dbCommand.Parameters.Add("@Expires", NpgsqlDbType.TimestampTZ).Value = DateTime.Now.AddMinutes(m_config.Timeout.Minutes);
					dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
					dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;

					NpgsqlTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbCommand.Prepare();

						dbTrans = dbConn.BeginTransaction();

						dbCommand.ExecuteNonQuery();

						// Attempt to commit the transaction
						dbTrans.Commit();
					}
					catch (Exception e)
					{
						Trace.WriteLine(e.ToString());

						if (dbTrans != null)
						{
							try
							{
								// Attempt to roll back the transaction
								Trace.WriteLine(Properties.Resources.LogRollbackAttempt);
								dbTrans.Rollback();
							}
							catch (Exception re)
							{
								// Rollback failed
								Trace.WriteLine(Properties.Resources.ErrRollbackFailed);
								Trace.WriteLine(re.ToString());
							}
						}

						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbTrans != null)
							dbTrans.Dispose();

						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}

		/// <summary>
		/// SessionStateProviderBase.SetAndReleaseItemExclusive
		/// </summary>
		public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			// Serialize the SessionStateItemCollection as a string
			string serializedItems = Serialize((SessionStateItemCollection)item.Items);

			using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
			{
				using (NpgsqlCommand dbCommand = dbConn.CreateCommand(),
						delCommand = dbConn.CreateCommand())
				{
					if (newItem)
					{
						// Delete existing expired session if exist
						delCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM \"{0}\" WHERE \"SessionId\" = @SessionId AND \"ApplicationName\" = @ApplicationName", s_tableName);

						delCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
						delCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;

						// Insert new session data
						dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"SessionId\", \"ApplicationName\", \"Created\", \"Expires\", \"Timeout\", \"Locked\", \"LockId\", \"LockDate\", \"Data\", \"Flags\") Values (@SessionId, @ApplicationName, @Created, @Expires, @Timeout, @Locked, @LockId, @LockDate, @Data, @Flags)", s_tableName);

						dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
						dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;
						dbCommand.Parameters.Add("@Created", NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
						dbCommand.Parameters.Add("@Expires", NpgsqlDbType.TimestampTZ).Value = DateTime.Now.AddMinutes((Double)item.Timeout);
						dbCommand.Parameters.Add("@Timeout", NpgsqlDbType.Integer).Value = item.Timeout;
						dbCommand.Parameters.Add("@Locked", NpgsqlDbType.Boolean).Value = false;
						dbCommand.Parameters.Add("@LockId", NpgsqlDbType.Integer).Value = 0;
						dbCommand.Parameters.Add("@LockDate", NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
						dbCommand.Parameters.Add("@Data", NpgsqlDbType.Text).Value = serializedItems;
						dbCommand.Parameters.Add("@Flags", NpgsqlDbType.Integer).Value = 0;
					}
					else
					{
						// Update existing session
						dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE \"{0}\" SET \"Expires\" = @Expires, \"Locked\" = @Locked, \"Data\" = @Data WHERE \"SessionId\" = @SessionId AND \"ApplicationName\" = @ApplicationName AND \"LockId\" = @LockId", s_tableName);

						dbCommand.Parameters.Add("@Expires", NpgsqlDbType.TimestampTZ).Value = DateTime.Now.AddMinutes((Double)item.Timeout);
						dbCommand.Parameters.Add("@Locked", NpgsqlDbType.Boolean).Value = false;
						dbCommand.Parameters.Add("@Data", NpgsqlDbType.Text).Value = serializedItems;
						dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
						dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;
						dbCommand.Parameters.Add("@LockId", NpgsqlDbType.Integer).Value = lockId;
					}

					NpgsqlTransaction dbTrans = null;

					try
					{
						dbConn.Open();
						dbTrans = dbConn.BeginTransaction();

						if (newItem)
						{
							delCommand.Prepare();
							delCommand.ExecuteNonQuery();
						}

						dbCommand.Prepare();
						dbCommand.ExecuteNonQuery();

						// Attempt to commit the transaction
						dbTrans.Commit();
					}
					catch (Exception e)
					{
						Trace.WriteLine(e.ToString());

						if (dbTrans != null)
						{
							try
							{
								// Attempt to roll back the transaction
								Trace.WriteLine(Properties.Resources.LogRollbackAttempt);
								dbTrans.Rollback();
							}
							catch (Exception re)
							{
								// Rollback failed
								Trace.WriteLine(Properties.Resources.ErrRollbackFailed);
								Trace.WriteLine(re.ToString());
							}
						}

						throw new ProviderException(Properties.Resources.ErrOperationAborted);
					}
					finally
					{
						if (dbTrans != null)
							dbTrans.Dispose();

						if (dbConn != null)
							dbConn.Close();
					}
				}
			}
		}

		/// <summary>
		/// SessionStateProviderBase.SetItemExpireCallback
		/// </summary>
		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			return false;
		}
		#endregion

		#region private methods

		/// <summary>
		/// Retrieves the session data from the data source.
		/// </summary>
		/// <param name="lockRecord">If true GetSessionStoreItem locks the record and sets a new LockId and LockDate.</param>	
		private SessionStateStoreData GetSessionStoreItem(bool lockRecord, HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
		{
			SessionStateStoreData result = null;
			lockAge = TimeSpan.Zero;
			lockId = null;
			locked = false;
			actionFlags = 0;
			DateTime expires = DateTime.MinValue;
			int timeout = 0;
			string serializedItems = null;

			using (NpgsqlConnection dbConn = new NpgsqlConnection(m_connectionString))
			{
				NpgsqlTransaction dbTrans = null;
				try
				{
					dbConn.Open();
					dbTrans = dbConn.BeginTransaction();

					// Retrieve the current session item information and lock row
					using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
					{
						dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT \"Expires\", \"Timeout\", \"Locked\", \"LockId\", \"LockDate\", \"Data\", \"Flags\" FROM \"{0}\" WHERE \"SessionId\" = @SessionId AND \"ApplicationName\" = @ApplicationName FOR UPDATE", s_tableName);

						dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
						dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;

						using (NpgsqlDataReader reader = dbCommand.ExecuteReader(System.Data.CommandBehavior.SingleRow))
						{
							while (reader.Read())
							{
								expires = reader.GetDateTime(0);
								timeout = reader.GetInt32(1);
								locked = reader.GetBoolean(2);
								lockId = reader.GetInt32(3);
								lockAge = DateTime.Now.Subtract(reader.GetDateTime(4));

								if (!reader.IsDBNull(5))
									serializedItems = reader.GetString(5);

								actionFlags = (SessionStateActions)reader.GetInt32(6);
							}
							reader.Close();
						}
					}

					// If record was not found, is expired or is locked, return.
					if (expires < DateTime.Now || locked)
						return result;

					// If the actionFlags parameter is not InitializeItem, deserialize the stored SessionStateItemCollection
					if (actionFlags == SessionStateActions.InitializeItem)
						result = CreateNewStoreData(context, m_config.Timeout.Minutes);
					else
						result = new SessionStateStoreData(Deserialize(serializedItems), SessionStateUtility.GetSessionStaticObjects(context), m_config.Timeout.Minutes);

					if (lockRecord)
					{
						lockId = (int)lockId + 1;
						// Obtain a lock to the record
						using (NpgsqlCommand dbCommand = dbConn.CreateCommand())
						{
							dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE \"{0}\" SET \"Locked\" = @Locked, \"LockId\" = @LockId,\"LockDate\" = @LockDate, \"Flags\" = @Flags WHERE \"SessionId\" = @SessionId AND \"ApplicationName\" = @ApplicationName", s_tableName);

							dbCommand.Parameters.Add("@Locked", NpgsqlDbType.Boolean).Value = true;
							dbCommand.Parameters.Add("@LockId", NpgsqlDbType.Integer).Value = lockId;
							dbCommand.Parameters.Add("@LockDate", NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
							dbCommand.Parameters.Add("@Flags", NpgsqlDbType.Integer).Value = 0;
							dbCommand.Parameters.Add("@SessionId", NpgsqlDbType.Varchar, 80).Value = id;
							dbCommand.Parameters.Add("@ApplicationName", NpgsqlDbType.Varchar, 255).Value = m_applicationName;

							dbCommand.ExecuteNonQuery();
						}
					}

					// Attempt to commit the transaction
					dbTrans.Commit();
				}
				catch (Exception e)
				{
					Trace.WriteLine(e.ToString());

					if (dbTrans != null)
					{
						try
						{
							// Attempt to roll back the transaction
							Trace.WriteLine(Properties.Resources.LogRollbackAttempt);
							dbTrans.Rollback();
						}
						catch (Exception re)
						{
							// Rollback failed
							Trace.WriteLine(Properties.Resources.ErrRollbackFailed);
							Trace.WriteLine(re.ToString());
						}
					}

					throw new ProviderException(Properties.Resources.ErrOperationAborted);
				}
				finally
				{
					if (dbTrans != null)
						dbTrans.Dispose();

					if (dbConn != null)
						dbConn.Close();
				}

				return result;
			}
		}

		/// <summary>
		/// Convert a SessionStateItemCollection into a Base64 string
		/// </summary>
		private string Serialize(SessionStateItemCollection items)
		{
			if (items == null || items.Count < 1)
				return string.Empty;

			using (MemoryStream mStream = new MemoryStream())
			{
				using (BinaryWriter bWriter = new BinaryWriter(mStream))
				{
					items.Serialize(bWriter);
					bWriter.Close();
				}

				return Convert.ToBase64String(mStream.ToArray());
			}
		}

		/// <summary>
		/// Convert a Base64 string into a SessionStateItemCollection
		/// </summary>
		/// <param name="serializedItems"></param>
		/// <returns></returns>
		private SessionStateItemCollection Deserialize(string serializedItems)
		{
			SessionStateItemCollection sessionItems = new SessionStateItemCollection();

			if (string.IsNullOrEmpty(serializedItems))
				return sessionItems;

			using (MemoryStream mStream = new MemoryStream(Convert.FromBase64String(serializedItems)))
			{
				using (BinaryReader bReader = new BinaryReader(mStream))
				{
					sessionItems = SessionStateItemCollection.Deserialize(bReader);
					bReader.Close();
				}
			}

			return sessionItems;
		}
		#endregion
	}
}
