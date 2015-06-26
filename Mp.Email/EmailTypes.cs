using System;
using Microsoft.SPOT;
using System.Collections;

namespace Mp.Email
{
    public sealed class EmailAddress
    {
        public string Name { get; private set; }
        public string Address { get; private set; }

        public EmailAddress(string Name, string Address)
        {
            this.Name = Name;
            this.Address = Address;
        }

        public EmailAddress()
            : this(string.Empty, string.Empty)
        { }
    }

    public sealed class EmailAddressCollection : IEnumerable
    {
        private ArrayList _emailCollection;

        public int Count { get { return _emailCollection.Count; } }

        public EmailAddressCollection()
        {
            _emailCollection = new ArrayList();
        }

        public void Add(EmailAddress address)
        {
            _emailCollection.Add(address);
        }

        public void AddRange(EmailAddressCollection addresses)
        {
            foreach (EmailAddress address in addresses)
                _emailCollection.Add(address);
        }

        public void Remove(EmailAddress address)
        {
            _emailCollection.Remove(address);
        }

        public void RemoveAt(int index)
        {
            _emailCollection.RemoveAt(index);
        }

        public IEnumerator GetEnumerator()
        {
            return _emailCollection.GetEnumerator();
        }

        public EmailAddress this[int i]
        {
            get { return (EmailAddress)_emailCollection[i]; }
            set { _emailCollection[i] = value; }
        }
    }

    public sealed class MailMessage
    {
        private EmailAddressCollection _to, _cc;
        private EmailAddress _from;
        private string _subject, _body;

        public EmailAddress From { get { return _from; } }
        public EmailAddressCollection To { get { return _to; } }
        public EmailAddressCollection Cc { get { return _cc; } }
        public string Subject { get { return _subject; } set { _subject = value; } }
        public string Body { get { return _body; } set { _body = value; } }


        public MailMessage(string from, string to, string subject, string body)
        {
            _from = new EmailAddress(from, from);
            _to = new EmailAddressCollection();
            _cc = new EmailAddressCollection();
            _to.Add(new EmailAddress(to, to));
            _subject = subject;
            _body = body;
        }
    }
}
