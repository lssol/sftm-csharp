//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Lucene.Net.Analysis;
//using Lucene.Net.Index;
//using Lucene.Net.Store;
//using Lucene.Net.Documents;
//using Lucene.Net.Search;
//using Lucene.Net.Util;
//
//namespace tree_matching_csharp
//{
//    public class FullTextSearch
//    {
//        private IndexWriter _writer;
//
//        public FullTextSearch()
//        {
//            var analyzer = new WhitespaceAnalyzer();
//            var mmap     = FSDirectory.Open(@"C:\Index");
//            
//            _writer = new IndexWriter(mmap, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
//        }
//
//        public void Test()
//        {
//            var source = new
//            {
//                Name           = "Kermit the Frog",
//                FavoritePhrase = "The quick brown fox jumps over the lazy dog"
//            };
//            var doc = new Document();
//            doc.Add(new Field("name", source.Name, Field.Store.YES, Field.Index.ANALYZED));
//            doc.Add(new Field("favoritePhrase", source.Name, Field.Store.YES, Field.Index.ANALYZED));
//
//            _writer.AddDocument(doc);
//            _writer.Flush(false,false, false);
//            
//            var phrase = new MultiPhraseQuery();
//            phrase.Add(new Term("favoritePhrase", "brown"));
//            phrase.Add(new Term("favoritePhrase", "fox"));
//
//            var searcher = new IndexSearcher(_writer.GetReader());
//            var hits     = searcher.Search(phrase, 20 /* top 20 */).ScoreDocs;
//            foreach (var hit in hits)
//            {
//                var foundDoc = searcher.Doc(hit.Doc); 
//                var score = hit.Score;
//                var name = foundDoc.Get("name");
//                var favoritePhrase = foundDoc.Get("favoritePhrase");
//            }
//
//        }
//        
//        public void Index(IEnumerable<Node> nodes)
//        {
//            var docs = nodes.Select(n =>
//            {
//                var doc = new Document();
//                doc.Add(new Field(nameof(n.Value), n.Value, Field.Store.NO, Field.Index.ANALYZED));
//                doc.Add(new Field(nameof(n.XPath), n.Value, Field.Store.NO, Field.Index.ANALYZED));
//
//                return new Document();
//            });
//
//            foreach (var doc in docs)
//                _writer.AddDocument(doc);
//            _writer.Flush(false, false, false);
//        }
//    }
//}