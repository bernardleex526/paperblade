import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "ci"))
import test_results as tr  # noqa: E402

XML_PASS = """<?xml version="1.0" encoding="utf-8"?>
<test-run id="2" testcasecount="3" result="Passed" total="3" passed="3" failed="0" inconclusive="0" skipped="0">
  <test-suite type="TestSuite"><test-case name="A" fullname="N.A" result="Passed"/></test-suite>
</test-run>"""

XML_FAIL = """<?xml version="1.0" encoding="utf-8"?>
<test-run id="2" result="Failed" total="2" passed="1" failed="1" inconclusive="0" skipped="0">
  <test-suite type="TestSuite">
    <test-case name="B" fullname="N.B" result="Failed"><failure><message><![CDATA[Expected 4 but was 3]]></message></failure></test-case>
  </test-suite>
</test-run>"""

XML_EMPTY = """<test-run id="2" result="Passed" total="0" passed="0" failed="0" inconclusive="0" skipped="0"/>"""

JSON_PASS = '{"Summary":{"Total":10,"Passed":10,"Failed":0,"Skipped":0,"Inconclusive":0},"Results":[],"success":true}'
JSON_ERROR = ('{"Summary":{"Total":0,"Passed":0,"Failed":0,"Skipped":0,"Inconclusive":0},"Results":[],'
              '"success":false,"error":"PlayMode tests cannot run synchronously"}')
JSON_FAIL = ('{"Summary":{"Total":2,"Passed":1,"Failed":1,"Skipped":0,"Inconclusive":0},'
             '"Results":[{"FullName":"N.C","Status":"Failed","Message":"boom"}],"success":true}')


class TestResultsTests(unittest.TestCase):
    def run_main(self, content: str, *extra: str) -> int:
        with tempfile.TemporaryDirectory() as d:
            p = Path(d) / "r.txt"
            p.write_text(content, encoding="utf-8")
            return tr.main([str(p), *extra])

    def test_xml_pass(self):
        self.assertEqual(self.run_main(XML_PASS), 0)

    def test_xml_failure_returns_1_and_collects_message(self):
        s = tr.parse_nunit_xml(XML_FAIL)
        self.assertEqual(s.failed, 1)
        self.assertEqual(s.failures[0], ("N.B", "Expected 4 but was 3"))
        self.assertEqual(self.run_main(XML_FAIL), 1)

    def test_zero_tests_is_failure(self):
        self.assertEqual(self.run_main(XML_EMPTY), 1)

    def test_min_total_enforced(self):
        self.assertEqual(self.run_main(XML_PASS, "--min-total", "4"), 1)
        self.assertEqual(self.run_main(XML_PASS, "--min-total", "3"), 0)

    def test_pipeline_json_pass(self):
        self.assertEqual(self.run_main(JSON_PASS), 0)

    def test_pipeline_success_false_is_failure_even_with_exit_0(self):
        s = tr.parse_pipeline_json(JSON_ERROR)
        self.assertIn("synchronously", s.error)
        self.assertEqual(self.run_main(JSON_ERROR), 1)

    def test_pipeline_failed_case(self):
        self.assertEqual(self.run_main(JSON_FAIL), 1)

    def test_pipeline_async_status_lowercase_keys(self):
        done = ('{"status":"completed","summary":{"total":9,"passed":9,"failed":0,"skipped":0,"inconclusive":0},'
                '"results":[]}')
        self.assertEqual(self.run_main(done), 0)
        s = tr.parse_pipeline_json(done)
        self.assertEqual((s.total, s.passed), (9, 9))

    def test_pipeline_async_status_not_completed_is_failure(self):
        running = '{"status":"running","summary":{"total":3,"passed":3,"failed":0,"skipped":0,"inconclusive":0}}'
        self.assertEqual(self.run_main(running), 1)

    def test_missing_file_returns_2(self):
        self.assertEqual(tr.main(["does/not/exist.xml"]), 2)

    def test_garbage_returns_2(self):
        self.assertEqual(self.run_main("not a result"), 2)


if __name__ == "__main__":
    unittest.main()
