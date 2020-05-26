namespace Hsf.DAL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class HsfDBContext : DbContext
    {
        public HsfDBContext()
            : base("name=HsfDBContext")
        {
        }

        public virtual DbSet<baidu_items> baidu_items { get; set; }
        public virtual DbSet<baidu_terms> baidu_terms { get; set; }
        public virtual DbSet<sound_fail> sound_fail { get; set; }
        public virtual DbSet<sound_host> sound_host { get; set; }
        public virtual DbSet<host_account> host_account { get; set; }
        public virtual DbSet<host_device> host_device { get; set; }
        public virtual DbSet<host_room> host_room { get; set; }
        public virtual DbSet<host_name> host_name { get; set; }
        public virtual DbSet<sound_title> sound_title { get; set; }
        public virtual DbSet<cust_answer> cust_answer { get; set; }
        public virtual DbSet<cust_qustion> cust_qustion { get; set; }
        public virtual DbSet<cust_word> cust_word { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<baidu_items>()
                .Property(e => e.wordid)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.titleid)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.formal)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.item)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.ne)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.pos)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.uri)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.loc_details)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.basic_words)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.vec)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.ext1)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.ext2)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.ext3)
                .IsUnicode(false);




            modelBuilder.Entity<baidu_terms>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_terms>()
                .Property(e => e.Message)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_terms>()
                .Property(e => e.Terms)
                .IsUnicode(false);




            modelBuilder.Entity<sound_fail>()
                .Property(e => e.sessionId)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.deviceId)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.actionId)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.token)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.questions)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.IPAddress)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.id)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.classfid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.deviceid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devip)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devmac)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devport)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devposition)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devregcode)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devtype)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.imageid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devstate)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devstate1)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devstate2)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.token)
                .IsUnicode(false);



            modelBuilder.Entity<host_account>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<host_account>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<host_account>()
                .Property(e => e.Mac)
                .IsUnicode(false);


            modelBuilder.Entity<host_device>()
                .Property(e => e.id)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.deviceid)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devtype)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devip)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devmac)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devport)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devposition)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devchannel)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.imageid)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.powvalue)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devstate)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devstate1)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devstate2)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.cachekey)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.id)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.posid)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.imageid)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.postype)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.Mac)
                .IsUnicode(false);

            modelBuilder.Entity<host_name>()
                .Property(e => e.HostName)
                .IsUnicode(false);

            modelBuilder.Entity<host_name>()
                .Property(e => e.CreateMac)
                .IsUnicode(false);



            modelBuilder.Entity<sound_title>()
                .Property(e => e.titleid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.titletext)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.preid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.nextid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.talkid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.sender)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.sendtype)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.field)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.talkstate)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.ext1)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.ext2)
                .IsUnicode(false);

            modelBuilder.Entity<sound_title>()
                .Property(e => e.ext3)
                .IsUnicode(false);


            modelBuilder.Entity<cust_answer>()
                .Property(e => e.answerid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.questionid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.answertext)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.answertype)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.preid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.nextid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.ext1)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.ext2)
                .IsUnicode(false);

            modelBuilder.Entity<cust_answer>()
                .Property(e => e.ext3)
                .IsUnicode(false);


            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.questionid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.questiontext)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.questiontype)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.preid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.nextid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.ext1)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.ext2)
                .IsUnicode(false);

            modelBuilder.Entity<cust_qustion>()
                .Property(e => e.ext3)
                .IsUnicode(false);


            modelBuilder.Entity<cust_word>()
                .Property(e => e.wordid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_word>()
                .Property(e => e.actionid)
                .IsUnicode(false);

            modelBuilder.Entity<cust_word>()
                .Property(e => e.wortdtext)
                .IsUnicode(false);

            modelBuilder.Entity<cust_word>()
                .Property(e => e.wordweigh)
                .IsUnicode(false);

            modelBuilder.Entity<cust_word>()
                .Property(e => e.ext1)
                .IsUnicode(false);

            modelBuilder.Entity<cust_word>()
                .Property(e => e.ext2)
                .IsUnicode(false);

            modelBuilder.Entity<cust_word>()
                .Property(e => e.ext3)
                .IsUnicode(false);
        }
    }
}
