using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Search;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;


namespace Mindscape.Lightspeed.Extensions
{
    public class LuceneSearch : ISearchEngine
    {
       
        protected  Directory LuceneDirectory;
        protected  Analyzer Analyzer = new StandardAnalyzer(Version.LUCENE_30);

        protected IndexWriter IndexWriter;
        protected bool CreateNewIndex;


        protected LuceneSearch()
        {
        }
       
        public LuceneSearch(Directory directory, Analyzer analyzer = null)
        {
            LuceneDirectory = directory;
            if (analyzer!= null)
            {
                Analyzer = analyzer;
            }
        }
        public LuceneSearch(Directory directory, ISet<String> stopWords)
        {
            LuceneDirectory = directory;
            Analyzer = new StandardAnalyzer(Version.LUCENE_30, stopWords);
        }


        public LightSpeedContext Context { get; set; }

        private IndexWriter GetIndexWriter()
        {
            IndexWriter indexWriter;

            try
            {
                indexWriter = new IndexWriter(LuceneDirectory, Analyzer, CreateNewIndex, IndexWriter.MaxFieldLength.UNLIMITED);
            }
            catch (System.IO.FileNotFoundException)
            {
                //Index does not exist yet. Create new index: true
                indexWriter = new IndexWriter(LuceneDirectory, Analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            }
            return indexWriter;
        }


        public void Add(IndexKey indexKey, string data)
        {
            Document doc = new Document();
            doc.Add(new Field("key", indexKey.Key, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("scope", indexKey.Scope, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("id", indexKey.EntityId, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("data", data, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));
            if (IndexWriter != null)
            {
                IndexWriter.AddDocument(doc, Analyzer);
            }
            else
            {
                using (IndexWriter indexWriter = GetIndexWriter())
                {
                    indexWriter.AddDocument(doc, Analyzer);
                }
            }
        }

        public void BeginBulkAdd()
        {
            IndexWriter = GetIndexWriter();
        }

        public void Clear()
        {
            CreateNewIndex = true;

            var files = LuceneDirectory.ListAll();
            foreach (var file in files)
            {
                LuceneDirectory.DeleteFile(file);
            }

            using (GetIndexWriter()) { }
        }


        public void EndBulkAdd()
        {
            CreateNewIndex = false;
            if (IndexWriter == null)
                return;

            IndexWriter.Dispose();
            IndexWriter = null;
        }

        public void Optimize()
        {
            using (IndexWriter indexWriter = GetIndexWriter())
            {
                indexWriter.Optimize();
            }
        }

        public void Remove(IndexKey indexKey)
        {
            using (IndexReader indexReader = IndexReader.Open(LuceneDirectory, false))
            {
                indexReader.DeleteDocuments(new Term("key", indexKey.Key));
            }
        }

        public void Update(IndexKey indexKey, string data)
        {
            Remove(indexKey);
            Add(indexKey, data);
        }

        public IList<SearchResult> Search(string query, params string[] scopes)
        {
            using (IndexSearcher indexSearcher = new IndexSearcher(LuceneDirectory))
            {
                QueryParser queryParser = new QueryParser(Version.LUCENE_30, "data", Analyzer);
                Query luceneQuery = queryParser.Parse(query);
                QueryWrapperFilter queryFilter = BuildQueryFilter(scopes);
                TopDocs hits = RunQuery(queryFilter, indexSearcher, luceneQuery);
                return ExtractSearchResults(hits, indexSearcher);
            }
        }

        private IList<SearchResult> ExtractSearchResults(TopDocs hits, IndexSearcher indexSearcher)
        {
            return hits.ScoreDocs
                     .Select(hit => new { hit, doc = indexSearcher.Doc(hit.Doc) })
                     .Select(s =>
                         new SearchResult(
                             s.doc.GetField("key").StringValue,
                             s.doc.GetField("scope").StringValue,
                             s.doc.GetField("id").StringValue,
                             s.hit.Score))
                     .ToList();
        }

        private TopDocs RunQuery(QueryWrapperFilter queryFilter, IndexSearcher indexSearcher, Query luceneQuery)
        {
            TopDocs hits = queryFilter != null
                ? indexSearcher.Search(luceneQuery, queryFilter, 1)
                : (indexSearcher).Search(luceneQuery, 1);
            return hits;
        }

        private QueryWrapperFilter BuildQueryFilter(string[] scopes)
        {
            if (scopes.Length <= 0) return null;

            BooleanQuery booleanQuery = new BooleanQuery();
            foreach (string scope in scopes)
            {
                booleanQuery.Add(new TermQuery(new Term("scope", scope)), Occur.SHOULD);
            }
            return new QueryWrapperFilter(booleanQuery);

        }

       
    }
}
