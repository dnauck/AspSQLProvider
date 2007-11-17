//
// $Id$
//
// Copyright © 2007 Nauck IT KG		http://www.nauck-it.de
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
using System.Text;
using System.Web.SessionState;

namespace NauckIT.PostgreSQLProvider
{
	public class PgSessionStateStoreProvider : SessionStateStoreProviderBase
	{
		public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void CreateUninitializedItem(System.Web.HttpContext context, string id, int timeout)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void Dispose()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void EndRequest(System.Web.HttpContext context)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override SessionStateStoreData GetItem(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override SessionStateStoreData GetItemExclusive(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void InitializeRequest(System.Web.HttpContext context)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void ReleaseItemExclusive(System.Web.HttpContext context, string id, object lockId)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void RemoveItem(System.Web.HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void ResetItemTimeout(System.Web.HttpContext context, string id)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void SetAndReleaseItemExclusive(System.Web.HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
