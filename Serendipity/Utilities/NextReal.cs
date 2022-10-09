using System.Data;
using System.Data.SqlClient;

namespace Serendipity.Utilities
{
    /// <summary>
    /// 열거형 연산기호 (<, <=, ==, >=, >, !=)
    /// </summary>
    public enum Kiho
    {
        /// <summary>
        /// 미만(<)
        /// </summary>
        Miman,
        /// <summary>
        /// 이하(<=)
        /// </summary>
        Iha,
        /// <summary>
        /// 같음(==)
        /// </summary>
        Gatum,
        /// <summary>
        /// 이상(>=)
        /// </summary>
        Isang,
        /// <summary>
        /// 초과(>)
        /// </summary>
        Choga,
        /// <summary>
        /// 다름(!=)
        /// </summary>
        Darum
    }

    /// <summary>
    /// 회차 전체데이터의 후방연속, 연속최대, 다음출현을 검사하는 클래스
    /// </summary>
    public static class NextReal
    {
        #region 필드

        private const string GoldenConnection = "Data Source=CHOI-PC;Initial Catalog=GoldenDB;Integrated Security=True";
        private const string LottoConnection = "Data Source=CHOI-PC;Initial Catalog=LottoDB;Integrated Security=True";
        
        /// <summary>
        /// 한계값 실수배열
        /// </summary>
        public static double[] LimitInts { get; } = { 0.01 * 0.33, 0.01 * 0.67, 0.01, 0.01 * 2, 0.01 * 3 };

        #endregion


        private static string EnumToString(Kiho kiho)
        {
            string s = kiho switch
            {
                Kiho.Miman => "<",
                Kiho.Iha => "<=",
                Kiho.Gatum => "=",
                Kiho.Isang => ">=",
                Kiho.Choga => ">",
                Kiho.Darum => "!=",
                _ => throw new Exception("기호에 오류가 있음."),
            };
            return s;
        }

        /// <summary>
        /// 최종출과 동일한 후방연속갯수
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <returns></returns>
        public static int CountOfRealSame(IEnumerable<int> ascCollection)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("컬렉션에 요소가 없습니다.");
            }

            int last = ascCollection.Last();
            int real = 0;

            foreach (int n in ascCollection.Reverse())
            {
                if (n == last)
                    real++;
                else
                    break;
            }

            return real;
        }

        /// <summary>
        /// 최종출과 동일한 후방연속갯수
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <returns></returns>
        public static int CountOfRealSame(IEnumerable<IEnumerable<int>> ascCollection)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("컬렉션에 요소가 없습니다.");
            }

            var last = ascCollection.Last();
            int real = 0;

            foreach (var n in ascCollection.Reverse())
            {
                if (n.SequenceEqual(last))
                    real++;
                else
                    break;
            }

            return real;
        }

        /// <summary>
        /// 후방동일 연속갯수 뺀 나머지에서 최종출과 동일한 연속최대 갯수
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <param name="realCount">후방연속갯수</param>
        /// <returns></returns>
        public static int CountOfMaxSame(IEnumerable<int> ascCollection, int realCount)
        {
            int max = 0, dup = 0;
            
            int last = ascCollection.Last();
            var lists = ascCollection.Take(ascCollection.Count() - realCount);

            foreach (int n in lists)
            {
                if (n == last)
                {
                    dup++;
                }
                else
                {
                    if (dup > max)
                    {
                        max = dup;
                    }

                    dup = 0;
                }
            }

            //만약 마지막 요소가 최종출과 같을때 루프 벗어났기 때문에 다시 검사해줌
            if (dup > max)
            {
                max = dup;
            }

            return max;
        }

        /// <summary>
        /// 후방동일 연속갯수 뺀 나머지에서 최종출과 동일한 연속최대 갯수
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <param name="realCount">후방연속갯수</param>
        /// <returns></returns>
        public static int CountOfMaxSame(IEnumerable<IEnumerable<int>> ascCollection, int realCount)
        {
            int max = 0, dup = 0;

            var last = ascCollection.Last();
            var lists = ascCollection.Take(ascCollection.Count() - realCount);

            foreach (var n in lists)
            {
                if (n.SequenceEqual(last))
                {
                    dup++;
                }
                else
                {
                    if (dup > max)
                    {
                        max = dup;
                    }

                    dup = 0;
                }
            }

            //만약 마지막 요소가 최종출과 같을때 루프 벗어났기 때문에 다시 검사해줌
            if (dup > max)
            {
                max = dup;
            }

            return max;
        }

        /// <summary>
        /// 최종출과 동일한 갯수
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <returns></returns>
        public static int CountOfLastSame(IEnumerable<int> ascCollection)
        {
            var last = ascCollection.Last();
            return ascCollection.Count(x => x == last);
        }

        /// <summary>
        /// 최종출과 동일한 갯수
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <returns></returns>
        public static int CountOfLastSame(IEnumerable<IEnumerable<int>> ascCollection)
        {
            var last = ascCollection.Last();
            return ascCollection.Count(x => x.SequenceEqual(last));
        }

        /// <summary>
        /// 데이터의 구간별출수합(최종값을 구간 -1의 합)
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <param name="section">검사구간 (기본값: 5)</param>
        /// <returns>오름차순 출수합 리스트</returns>
        public static List<int> GetSectionCount(IEnumerable<int> ascCollection, int section = 5)
        {
            var list = new List<int>();
            int[] descArray = ascCollection.Reverse().ToArray();

            //최종 검사구간 -1 의 데이터 
            int[] firstInts = descArray[^section..];
            list.Add(firstInts.Sum());

            for (int i = section - 1; i < descArray.Length; i += section)
            {
                Range range = new(i, section + i);

                try
                {
                    int[] ints = descArray[range];

                    if (ints.Length != section)
                        break;
                    else
                        list.Add(ints.Sum());
                }
                catch (Exception)
                {
                    break;
                }
            }
            list.Reverse();
            return list;
        }

        /// <summary>
        /// 최종출의 동출수, 출현간격
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <returns></returns>
        public static (int sameCount, List<int> gaps) GetGapList(IEnumerable<int> ascCollection)
        {
            var last = ascCollection.Last();
            var idxs = ascCollection.Select((n, i) => (n, i)).Where(x => x.n == last).Select(x => x.i).ToList();
            int cnt = idxs.Count;

            //최종출을 더함 (죄종출과 같은 인덱스를 찾기 때문에 최종출 + 1)
            idxs.Add(ascCollection.Count());

            var zip = idxs.Zip(idxs.Skip(1), (a, b) => b - a).ToList();
            
            //첫출을 삽입 (인덱스므로 +1)
            zip.Insert(0, idxs.First() + 1);

            return (cnt, zip);
        }

        /// <summary>
        /// 최종출의 동출수, 출현간격
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <returns></returns>
        public static (int sameCount, List<int> gaps) GetGapList(IEnumerable<IEnumerable<int>> ascCollection)
        {
            var last = ascCollection.Last();
            var idxs = ascCollection.Select((n, i) => (n, i)).Where(x => x.n.SequenceEqual(last)).Select(x => x.i).ToList();
            int cnt = idxs.Count;

            //최종출을 더함 (죄종출과 같은 인덱스를 찾기 때문에 최종출 + 1)
            idxs.Add(ascCollection.Count());

            var zip = idxs.Zip(idxs.Skip(1), (a, b) => b - a).ToList();

            //첫출을 삽입 (인덱스므로 +1)
            zip.Insert(0, idxs.First() + 1);

            return (cnt, zip);
        }

        /// <summary>
        /// 최종출의 동출수, 출현간격
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <param name="value">검사할 값</param>
        /// <returns></returns>
        public static (int sameCount, List<int> gaps) GetGapList(IEnumerable<int> ascCollection, int value)
        {
            var idxs = ascCollection.Select((n, i) => (n, i)).Where(x => x.n == value).Select(x => x.i).ToList();
            int cnt = idxs.Count;

            //최종출을 더함 (죄종출과 같은 인덱스를 찾기 때문에 최종출 + 1)
            idxs.Add(ascCollection.Count());

            var zip = idxs.Zip(idxs.Skip(1), (a, b) => b - a).ToList();

            //첫출을 삽입 (인덱스므로 +1)
            zip.Insert(0, idxs.First() + 1);

            return (cnt, zip);
        }

        /// <summary>
        /// 최종출의 동출수, 출현간격
        /// </summary>
        /// <param name="ascCollection">오름차순 전체데이터</param>
        /// <param name="value">검사할 값</param>
        /// <returns></returns>
        public static (int sameCount, List<int> gaps) GetGapList
                      (IEnumerable<IEnumerable<int>> ascCollection, IEnumerable<int> value)
        {
            var idxs = ascCollection.Select((n, i) => (n, i)).Where(x => x.n.SequenceEqual(value)).Select(x => x.i).ToList();
            int cnt = idxs.Count;

            //최종출을 더함 (죄종출과 같은 인덱스를 찾기 때문에 최종출 + 1)
            idxs.Add(ascCollection.Count());

            var zip = idxs.Zip(idxs.Skip(1), (a, b) => b - a).ToList();

            //첫출을 삽입 (인덱스므로 +1)
            zip.Insert(0, idxs.First() + 1);

            return (cnt, zip);
        }

        /// <summary>
        /// 끝출의 출현간격의 최소, 최대, 최종간격, 동격갯수, 제외여부
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터</param>
        /// <param name="limit">간격출현갯수 한계값 (기본값: 1)</param>
        /// <returns>튜플(최소간격, 최대간격, 최종간격, 동간격갯수, 제외여부)</returns>
        public static (int minGap, int maxGap, int lastGap, int countGap, bool isbad) MinMaxGapCount(IEnumerable<int> ascCollection)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int last = ascCollection.Last();
            int min = 0, max = 0, cnt = 0, gap = 0;
            bool bad = false;
            int limit = ascCollection.Max() switch
            {
                <= 4 => Convert.ToInt32(ascCollection.Count() * 0.01 * 2),
                <= 12 => Convert.ToInt32(ascCollection.Count() * 0.01 * 1.2),
                <= 50 => Convert.ToInt32(ascCollection.Count() * 0.01 * 0.9),
                <= 100 => Convert.ToInt32(ascCollection.Count() * 0.01 * 0.6),
                _ => Convert.ToInt32(ascCollection.Count() * 0.01 * 0.3)
            };

            //값과 인덱스
            var finds = ascCollection.Select((n, i) => (n, i)).Where(x => x.n == last).Select(x => x.i);

            if (finds.Any())
            {
                var zip = finds.Zip(finds.Skip(1), (a, b) => b - a).ToList();
                zip.Insert(0, finds.First() + 1);

                if (zip.Count >= limit)
                {
                    min = zip.Min();
                    max = zip.Max();
                    gap = zip.Last();
                    cnt = zip.Count(x => x == gap);
                    var (r, m) = RealMaxCount(zip);

                    if (r > m)
                    {
                        bad = true;
                    }
                }
            }

            return (min, max, gap, cnt, bad);
        }

        /// <summary>
        /// 데이터의 호악번 인덱스
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터</param>
        /// <returns>-1: 악번, 0: 무시, 1: 호번</returns>
        public static int FindBadGoodNumber(IEnumerable<int> ascCollection)
        {
            int num = 0;    //bad: -1   good: 1     ignorer: 0
            int last = ascCollection.Last();
            int min = ascCollection.Min();
            int max = ascCollection.Max();

            //현재 마지막 출수에 대한 후방,최대,다음리스트
            var (real, maxs) = RealMaxCount(ascCollection);
            int limit = Convert.ToInt32(ascCollection.Count() * LimitInts[3]);
            int same = CountOfLastSame(ascCollection);

            if (same >= limit)
            {
                if (real >= max)
                {
                    if (last == 0)
                        num = 1;
                    else
                        num = -1;
                }
                else
                {
                    //0출 연속인지 !0출 연속인지 판별
                    var (realCount, maxCount, nextList) = (last == 0) ? RealMaxNextList(ascCollection, Kiho.Gatum, 0) :
                                                                        RealMaxNextList(ascCollection, Kiho.Darum, 0);

                    if (last != 0 && nextList.All(x => x < last))
                    {
                        num = -1;
                    }
                    else
                    {
                        if (realCount > 3)
                        {
                            if (realCount >= maxCount)
                            {
                                if (last == 0)
                                    num = 1;
                                else
                                    num = -1;
                            }
                        }
                        else
                        {
                            if (nextList.All(x => x > 0) || nextList.All(x => x > last))
                            {
                                num = 1;
                            }
                            else if (nextList.All(x => x < 1) || nextList.All(x => x < last))
                            {
                                num = -1;
                            }
                            else
                            {
                                if (realCount >= maxCount)
                                {
                                    if (last == 0)
                                        num = 1;
                                    else
                                        num = -1;

                                }
                                else
                                {
                                    int nst = nextList.Last();
                                    var tpl = nst == 0 ? RealMaxCount(nextList, Kiho.Gatum, 0) : RealMaxCount(nextList, Kiho.Darum, 0);

                                    if (tpl.realCount >= tpl.maxCount && nst > 0 && last > 0)
                                        num = -1;
                                    else if (tpl.realCount >= tpl.maxCount && nst == 0 && last == 0)
                                        num = 1;
                                    else
                                        num = 0;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                num = 0;
            }

            return num;
        }

        /// <summary>
        /// 후방패턴 검사결과 호악번 인덱스
        /// </summary>
        /// <param name="ascCollection">오름찬순 데이터</param>
        /// <param name="patternCount">후방패턴 갯수</param>
        /// <returns>1: 악번, 0: 무시, 1: 호번</returns>
        public static int FindVerticalFiveData(IEnumerable<int> ascCollection, int patternCount)
        {
            int num = 0;
            var lastfive = ascCollection.Skip(ascCollection.Count() - 5);
            int dist = lastfive.Distinct().Count();

            if (dist >= 3)
            {
                var list = new List<int>();

                for (int i = 1; i <= patternCount; i++)
                {
                    var nexts = RealMaxNextList(ascCollection, i).nextList;

                    if (nexts.Any())
                        list.Add(nexts.Last());
                    else
                        list.Add(-1);

                }

                //다음출 5개 전부가 1이상이면 호번
                if (list.All(x => x == 0))
                {
                    num = -1;
                }
                else
                {
                    int cnt = list.Count(x => x != 0);

                    if (cnt >= 4)
                        num = 1;
                    else
                        num = 0;
                    
                }                
            }

            return num;
        }


        public static void DataOfLimitRange(IEnumerable<int> ascCollection, int limit, int bottom, int top)
        {
            if (top - 1 <= bottom)
            {
                throw new Exception("최저, 최고 설정오류.");
            }

            if (!(ascCollection?.Any() ?? false))
            {
                throw new Exception("빈 컬렉션입니다.");
            }

            int low = 0, high = 0;
            int count = ascCollection.Distinct().Count();

            if (count <= 1)
            {
                throw new Exception("고유 요소 없습니다.");
            }
            else if (count < 3)
            {
                low = ascCollection.Min() < 0 ? 0 : ascCollection.Min(); 
                high = ascCollection.Max();
            }
            else
            {
                int sum = 0, min = 0, max = 0;
                var cumuls = Enumerable.Range(bottom, top - bottom + 1).Select(x => (chul: x, cnt: ascCollection.Count(g => g == x)));

                foreach (var (chul, cnt) in cumuls)
                {
                    sum += cnt;
                    if (limit < sum)
                    {
                        min = chul;
                        break;
                    }
                }
                sum = 0;
                foreach (var (chul, cnt) in cumuls.AsEnumerable().Reverse())
                {
                    sum += cnt;
                    if (limit < sum)
                    {
                        max = chul;
                        break;
                    }
                }

                //출수최대값 - 출수최소값 차이가 2 이상이면
                if (2 <= max - min)
                {
                    var exp = cumuls.Where(x => x.cnt == 0 && x.chul > min && x.chul < max).Select(x => x.chul).ToList();

                    //후방연속 초과검사
                    var (realcon, maxcon, nexts) = RealMaxNextList(ascCollection);
                    int last = ascCollection.Last();
                    if ((last < min && last < max) && realcon >= maxcon)
                    {
                        exp.Add(last);
                    }

                    //다음출에서 초과검사
                    if ((nexts?.Any() ?? false) && limit * 2 <= nexts.Count)
                    {
                        int nxlast = nexts[^1];
                        var (realCount, maxCount) = NextReal.RealMaxCount(nexts);
                        if ((nxlast < min && last < max) && realCount >= maxCount)
                        {
                            exp.Add(nxlast);
                        }

                        var nxcnts = Enumerable.Range(min, max - min + 1).Select(x => (nxch: x, nxct: nexts.Count(g => g == x)));
                        var zeros = nxcnts.Where(x => x.nxct == 0 && x.nxch > min && x.nxch < max).Select(x => x.nxch);
                        if (zeros.Any())
                        {
                            foreach (int n in zeros)
                            {
                                int chulcount = cumuls.Where(x => x.chul == n).Single().cnt;
                                if (chulcount < count / 5)
                                {
                                    exp.Add(n);
                                }
                            }
                        }
                    }

                    low = min;
                    high = max;
                    //exp.Distinct().OrderBy(x => x).ToList());
                }
                else
                {
                    //최대값과 최소값 차이가 2 미만이면
                    low = ascCollection.Min() < 0 ? 0 : ascCollection.Min();
                    high = ascCollection.Max();
                }
            }
        }

        /// <summary>
        /// 컬렉션의 한계 범위
        /// </summary>
        /// <param name="ascCollection">오름차순 출수데이터</param>
        /// <param name="bottom">극저값(이론상 나올수 있는 최저값)</param>
        /// <param name="top">극고값(이론상 나올수 있는 최고값)</param>
        /// <returns>튜플(최저값, 최고값, 0출 리스트)</returns>
        public static (int low, int high, List<int> zerochul) GetCollectionRange(IEnumerable<int> ascCollection, int bottom = 0, int top = 6)
        {
            if (!(ascCollection?.Any() ?? false))
            {
                throw new Exception("빈 컬렉션입니다.");
            }
            if (top - 1 <= bottom)
            {
                throw new Exception("최저, 최고 설정오류.");
            }

            int low = 0, high = 0;
            (int chul, int cnt) rst = (-1, -1);
            var list = new List<int>();
            double[] limits = { 0.01 * 0.3, 0.01 * 0.6, 0.01 * 0.9, 0.01 * 1.2, 0.01 * 2 };
            int count = ascCollection.Distinct().Count();
            if (count <= 1)
            {
                throw new Exception("고유 요소 없습니다.");
            }
            else if (count < 3)
            {
                low = ascCollection.Min() < 0 ? 0 : ascCollection.Min();
                high = ascCollection.Max();
                int last = ascCollection.Last();
                rst = (last, ascCollection.Count(x => x == last));
            }
            else
            {
                int limit = count switch
                {
                    <= 004 => Convert.ToInt32(ascCollection.Count() * limits[4]),
                    <= 012 => Convert.ToInt32(ascCollection.Count() * limits[3]),
                    <= 050 => Convert.ToInt32(ascCollection.Count() * limits[2]),
                    <= 100 => Convert.ToInt32(ascCollection.Count() * limits[1]),
                    _ => Convert.ToInt32(ascCollection.Count() * limits[0])
                };

                int sum = 0, min = 0, max = 0;
                var cumuls = Enumerable.Range(bottom, top - bottom + 1).Select(x => (chul: x, cnt: ascCollection.Count(g => g == x))).ToList();

                foreach (var (chul, cnt) in cumuls)
                {
                    sum += cnt;
                    if (limit < sum)
                    {
                        min = chul;
                        break;
                    }
                }
                sum = 0;
                foreach (var (chul, cnt) in cumuls.AsEnumerable().Reverse())
                {
                    sum += cnt;
                    if (limit < sum)
                    {
                        max = chul;
                        break;
                    }
                }
                low = min;
                high = max;
                rst = cumuls.Where(x => x.chul == ascCollection.Last()).Single();
                list = cumuls.Where(x => x.cnt == 0 && x.chul > min && x.chul < max).Select(x => x.chul).ToList();
            }

            return (low, high, list);
        }
        
        /// <summary>
        /// 전체데이터의 하한, 상한, 제외리스트
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터</param>
        /// <returns>튜플(하한, 상한, 제외리스트)</returns>
        public static (int lowValue, int highValue, List<int> exceptList) FindMinMaxExceptList(IEnumerable<int> ascCollection)
        {
            var (low, high, exceptList) = NowChcekData(ascCollection);
            
            //다음 출수 검사
            var nexts = NextList(ascCollection);

            if (nexts is not null && nexts.Any())
            {
                int cnt = nexts.Count(x => x == nexts.Last());
                int ant = ascCollection.Count(x => x == nexts.Last());

                if (cnt <= ascCollection.Count() * 0.01 && ant <= ascCollection.Count() * 0.1)
                {
                    int last = nexts.Last();
                    var (realCount, maxCount) = RealMaxCount(nexts);

                    if (realCount >= maxCount)
                    {
                        if (!exceptList.Contains(last) && (last > low && last < high))
                        {
                            exceptList.Add(last);
                        }
                    }
                }
            }
            
            var tpl = ChangeMinMaxExceptList(low, high, exceptList);

            return tpl;
        }

        /// <summary>
        /// 전체데이터의 하한, 상한, 제외리스트 (이전회차와 비교검사 에 사용)
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터</param>
        /// <param name="numLengh">검사번호의 길이</param>
        /// <returns>튜플(하한, 상한, 제외리스트)</returns>
        public static (int lowValue, int highValue, List<int> exceptList) FindMinMaxExceptList(IEnumerable<int> ascCollection, int numLengh)
        {
            if (numLengh < 1)
            {
                return (0, 0, new List<int>());
            }
            else if (numLengh == 1)
            {
                return (0, 1, new List<int>());
            }
            else
            {
                var (low, high, exceptList) = NowChcekData(ascCollection);

                if (numLengh <= high)
                {
                    high = numLengh;
                }

                //다음 출수 검사
                var nexts = NextList(ascCollection);

                if (nexts is not null && nexts.Any())
                {
                    int cnt = nexts.Count(x => x == nexts.Last());
                    int ant = ascCollection.Count(x => x == nexts.Last());

                    if (cnt <= ascCollection.Count() * 0.01 && ant <= ascCollection.Count() * 0.1)
                    {
                        int last = nexts.Last();
                        var (realCount, maxCount) = RealMaxCount(nexts);

                        if (realCount >= maxCount)
                        {
                            if (!exceptList.Contains(last) && (last > low && last < high))
                            {
                                exceptList.Add(last);
                            }
                        }
                    }
                }

                var tpl = ChangeMinMaxExceptList(low, high, exceptList);

                return tpl;
            }
        }

        /// <summary>
        /// 현재출수 기준으로 하한, 상한, 제외리스트
        /// </summary>
        /// <param name="ascCollection"></param>
        /// <returns></returns>
        private static (int low, int high, List<int> exceptList) NowChcekData(IEnumerable<int> ascCollection)
        {
            int min = ascCollection.Min();
            int max = ascCollection.Max();
            int lastNumber = ascCollection.Last();
            var pairs = new List<(int key, int val, int limit)>();
            int datacnt = ascCollection.Count();

            //최저와 최고 사이값의 갯수
            for (int i = min; i <= max; i++)
            {
                int n = ascCollection.Count(x => x == i);

                int lmt = max switch
                {
                    > 80 => Convert.ToInt32(LimitInts[0] * datacnt),
                    > 10 => Convert.ToInt32(LimitInts[1] * datacnt),
                    _ => Convert.ToInt32(LimitInts[2] * datacnt)
                };

                pairs.Add((i, n, lmt));
            }

            if (pairs.Count >= 3)
            {
                var chgpairs = GetAccumulatList(pairs);
                min = chgpairs.Min(x => x.key);
                max = chgpairs.Max(x => x.key);
                var (sameCount, gaps) = GetGapList(ascCollection);
                var zerokeys = chgpairs.Where(x => x.val == 0).Select(x => x.key).ToList();

                if (sameCount >= LimitInts[3] * datacnt)
                {
                    var (realCount, maxCount) = RealMaxCount(ascCollection);

                    if (realCount >= maxCount || gaps.Last() == gaps.Max())
                    {
                        zerokeys.Add(lastNumber);
                    }
                }

                var tpl = ChangeMinMaxExceptList(min, max, zerokeys);
                return tpl;
            }
            else
            {
                return (min, max, new List<int>());
            }
        }

        /// <summary>
        /// 하한과 상한의 누적한계값 이내의 튜플리스트
        /// </summary>
        /// <param name="lists">튜플리스트</param>
        /// <param name="limit">한계값</param>
        /// <returns></returns>
        private static List<(int key, int val)> GetAccumulatList(List<(int key, int val, int limit)> lists)
        {
            int lowsum = 0, highsum = 0;
            int low = 0, hgh = 1;

            //하한
            for (int i = 0; i < lists.Count - 1; i++)
            {
                var (key, val, lmt) = lists[i];
                lowsum += val;

                if (lowsum >= lmt)
                {
                    low = key;
                    break;
                }
            }

            //상한
            for (int i = lists.Count - 1; i > 0; i--)
            {
                var (key, val, lmt) = lists[i];
                highsum += val;

                if (highsum >= lmt)
                {
                    hgh = key;
                    break;
                }
            }

            var rst = lists.Where(x => x.key >= low && x.key <= hgh).Select(x => (x.key, x.val)).ToList();

            return rst;
        }

        /// <summary>
        /// 하한과 상한의 누적한계값 이내의 튜플리스트
        /// </summary>
        /// <param name="lists">튜플리스트</param>
        /// <param name="limit">한계값</param>
        /// <returns></returns>
        private static List<(int key, int val)> GetAccumulatList(List<(int key, int val)> lists, int limit)
        {
            int lowsum = 0, highsum = 0;
            int low = 0, hgh = 1;

            //하한
            for (int i = 0; i < lists.Count - 1; i++)
            {
                var (key, val) = lists[i];
                lowsum += val;

                if (lowsum >= limit)
                {
                    low = key;
                    break;
                }
            }

            //상한
            for (int i = lists.Count - 1; i > 0; i--)
            {
                var (key, val) = lists[i];
                highsum += val;

                if (highsum >= limit)
                {
                    hgh = key;
                    break;
                }
            }

            var rst = lists.Where(x => x.key >= low &&  x.key <= hgh).ToList();

            return rst;
        }

        /// <summary>
        /// 제외번호가 하한과 상한 연속부분을 제거한 하한, 상한, 제외리스트
        /// </summary>
        /// <param name="min">하한값</param>
        /// <param name="max">상한값</param>
        /// <param name="lists">제외리스트</param>
        /// <returns></returns>
        private static (int low, int high, List<int> exceptList) ChangeMinMaxExceptList
                       (int min, int max, List<int> lists)
        {
            if (!lists?.Any() ?? false)
            {
                return (min, max, new List<int>());
            }
            else
            {
                //하단연속이나 상단연속이면 상하조절
                if (lists.Count == 1)
                {
                    int n = lists.First();
                    var temp = new List<int>();

                    if (min == n)
                    {
                        min++;
                    }
                    else if (max == n)
                    {
                        max--;
                    }
                    else if (min + 1 == n)
                    {
                        min = n;
                    }
                    else if (max - 1 == n)
                    {
                        max = n;
                    }
                    else
                    {
                        temp = new List<int>(lists);
                    }

                    return (min, max, temp);
                }
                else
                {
                    if (lists.Min() == min)
                    {
                        var temp = lists.Where(x => x != min).ToList();
                        return (min + 1, max, temp);
                    }
                    else if (lists.Max() == max)
                    {
                        var temp = lists.Where(x => x != max).ToList();
                        return (min, max - 1, temp);
                    }
                    else
                    {
                        int lowplus = 0, hghplus = 0;

                        //하단에서부터 검사
                        for (int i = 0; i < lists.Count - 1; i++)
                        {
                            int n = lists[i];

                            if (min + (i + 1) == n)
                                lowplus++;
                            else
                                break;
                        }

                        //상단에서부터 검사
                        var rev = lists.AsEnumerable().Reverse().ToList();

                        for (int i = 0; i < rev.Count - 1; i++)
                        {
                            int n = rev[i];

                            if (max - (i + 1) == n)
                                hghplus++;
                            else
                                break;
                        }

                        min += lowplus;
                        max -= hghplus;

                        //중복되것 삭제
                        var exp = lists.Where(x => x > min && x < max).ToList();
                        exp.Sort();

                        return (min, max, exp);
                    }
                }
            }
        }

        /// <summary>
        /// 최종출과 동일한 다음출 리스트
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터</param>
        /// <returns></returns>
        public static IEnumerable<int> NextList(IEnumerable<int> ascCollection)
        {
            var list = ascCollection.ToList();
            int real = CountOfRealSame(ascCollection);
            int last = ascCollection.Last();
            var remind = ascCollection.Take(ascCollection.Count() - real).ToList();

            var idxs = remind.Select((val, idx) => (val, idx)).Where(x => x.val == last)
                             .Select(x => x.idx).Where(x => x < remind.Count);
            
            var rst = new List<int>();

            foreach (int idx in idxs)
            {
                int sameCount = 0;

                for (int i = 0; i < real; i++)
                {
                    int a = idx + i;
                    int n = list[a];

                    if (n == last)
                        sameCount++;
                    else
                        break;
                }

                if (sameCount == real)
                {
                    int a = idx + real;
                    int n = list[a];
                    rst.Add(n);
                }
            }

            return rst;
        }


        /// <summary>
        /// 후방연속, 연속최대 갯수 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터 배열</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수)</returns>
        public static (int realCount, int maxCount) RealMaxCount(IEnumerable<int> ascCollection)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int[] ascArray = ascCollection.ToArray();

            if (DistinctList(ascArray).Count >= 2)
            {
                int real = 0;
                int last = ascArray[^1];

                //후방연속
                foreach (int n in ascCollection.Reverse())
                {
                    if (last == n)
                        real++;
                    else
                        break;
                }

                //연속최대
                int[] ascRemind = ascArray[..^real];
                int max = 0, dup = 0;

                foreach (int n in ascRemind)
                {
                    if (n == last)
                    {
                        dup++;
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                return (real, max);
            }
            else
            {
                return (ascArray.Length, -1);
            }
        }

        /// <summary>
        /// 후방연속, 연속최대 갯수 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터 리스트</param>
        /// <param name="kiho">검사부호</param>
        /// <param name="number">검사번호</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수)</returns>
        public static (int realCount, int maxCount) RealMaxCount(IEnumerable<int> ascCollection, Kiho kiho, int number)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int[] ascArray = ascCollection.ToArray();

            if (DistinctList(ascArray).Count >= 2)
            {
                int real = 0;

                //후방연속
                for (int i = ascArray.Length - 1; i >= 0; i--)
                {
                    if (IsSameOperation(kiho, number, ascArray[i]))
                        real++;
                    else
                        break;
                }

                //연속최대
                int[] ascRemind = ascArray[..^real];
                int max = 0, dup = 0;

                foreach (int n in ascRemind)
                {
                    if (IsSameOperation(kiho, number, n))
                    {
                        dup++;
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                return (real, max);
            }
            else
            {
                return (ascArray.Length, -1);
            }
        }

        /// <summary>
        /// 후방연속, 연속최대 갯수 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터 배열</param>
        /// <param name="patternCount">후방패턴 검사갯수 (1 이상)</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수)</returns>
        public static (int realCount, int maxCount) RealMaxCount(IEnumerable<int> ascCollection, int patternCount)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int[] ascArray = ascCollection.ToArray();

            if (DistinctList(ascArray).Count >= 2)
            {
                var realpattern = ascArray[^patternCount..];
                var revspattern = realpattern.AsEnumerable().Reverse().ToArray();
                int real = 0;

                //후방연속
                for (int i = ascArray.Length - 1; i >= 0; i -= patternCount)
                {
                    int sameCount = 0;

                    for (int j = 0; j < patternCount; j++)
                    {
                        int a = i - j;
                        int n = ascArray[a];

                        if (revspattern[j] == n)
                            sameCount++;
                        else
                            break;
                    }

                    if (sameCount == patternCount)
                        real++;
                    else
                        break;
                }

                int max = 0, dup = 0;
                int ix;

                //연속최대
                for (ix = 0; ix < ascArray.Length - patternCount; ix++)
                {
                    int sameCount = 0;
                    for (int j = 0; j < patternCount; j++)
                    {
                        int a = ix + j;
                        int n = ascArray[a];

                        if (realpattern[j] == n)
                            sameCount++;
                        else
                            break;
                    }

                    if (sameCount == patternCount)
                    {
                        dup++;
                        ix += patternCount - 1;
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                return (real, max);
            }
            else
            {
                return (ascArray.Length, -1);
            }
        }

        /// <summary>
        /// 후방연속, 연속최대 갯수 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollections">오름차순 데이터 배열 리스트</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수)</returns>
        public static (int realCount, int maxCount) RealMaxCount(IEnumerable<int[]> ascCollections)
        {
            if (!ascCollections?.Any() ?? false)
            {
                throw new Exception("배열리스트에 요소가 없음.");
            }

            List<int[]> ascLists = ascCollections.ToList();

            if (DistinctList(ascLists).Count >= 2)
            {
                int real = 0;
                int[] last = ascLists[^1];

                //후방연속
                for (int i = ascLists.Count - 1; i >= 0; i--)
                {
                    if (last.SequenceEqual(ascLists[i]))
                        real++;
                    else
                        break;
                }

                //연속최대
                int[][] ascRemind = ascLists.ToArray()[..^real];
                int max = 0, dup = 0;

                foreach (int[] n in ascRemind)
                {
                    if (n.SequenceEqual(last))
                    {
                        dup++;
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                return (real, max);
            }
            else
            {
                return (ascLists.Count, -1);
            }
        }

        /// <summary>
        /// 후방연속, 연속최대 갯수 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollections">오름차순 데이터 배열리스트</param>
        /// <param name="patternCount"></param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수)</returns>
        public static (int realCount, int maxCount) RealMaxCount(IEnumerable<int[]> ascCollections, int patternCount)
        {
            if (!ascCollections?.Any() ?? false)
            {
                throw new Exception("배열리스트에 요소가 없습니다.");
            }

            var ascLists = ascCollections.ToList();

            if (DistinctList(ascLists).Count >= 2)
            {
                var realpattern = ascLists.ToArray()[^patternCount..];
                var revspattern = realpattern.AsEnumerable().Reverse().ToArray();
                int real = 0;

                //후방연속
                for (int i = ascLists.Count - 1; i >= 0; i -= patternCount)
                {
                    int sameCount = 0;

                    for (int j = 0; j < patternCount; j++)
                    {
                        int a = i - j;
                        int[] n = ascLists[a];

                        if (revspattern[j].SequenceEqual(n))
                            sameCount++;
                        else
                            break;
                    }

                    if (sameCount == patternCount)
                        real++;
                    else
                        break;
                }

                int max = 0, dup = 0;
                int ix;

                //연속최대
                for (ix = 0; ix < ascLists.Count - patternCount; ix++)
                {
                    int sameCount = 0;
                    for (int j = 0; j < patternCount; j++)
                    {
                        int a = ix + j;
                        int[] n = ascLists[a];

                        if (realpattern[j].SequenceEqual(n))
                            sameCount++;
                        else
                            break;
                    }

                    if (sameCount == patternCount)
                    {
                        dup++;
                        ix += patternCount - 1;
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                return (real, max);
            }
            else
            {
                return (ascLists.Count, -1);
            }
        }

        /// <summary>
        /// 후방연속, 연속최대 갯수 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollections">오름차순 데이터 배열리스트</param>
        /// <param name="tpl">튜플 리스트 (검사부호, 검사번호)</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수)</returns>
        public static (int realCount, int maxCount) RealMaxCount
                      (IEnumerable<int[]> ascCollections, List<(Kiho kiho, int number)> tpl)
        {
            if (!ascCollections?.Any() ?? false)
            {
                throw new Exception("배열리스트에 요소가 없음.");
            }

            var ascLists = ascCollections.ToList();

            if (ascLists[0].Length != tpl.Count)
            {
                throw new Exception("데이터갯수와 조건갯수가 다름.");
            }

            if (DistinctList(ascLists).Count >= 2)
            {
                int real = 0, max = 0, dup = 0;

                //후방연속
                for (int i = ascLists.Count - 1; i >= 0; i--)
                {
                    if (IsPassOfArray(ascLists[i], tpl))
                        real++;
                    else
                        break;
                }

                //연속최대
                int[][] ascRemind = ascLists.ToArray()[..^real];

                foreach (int[] n in ascRemind)
                {
                    if (IsPassOfArray(n, tpl))
                    {
                        dup++;
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                return (real, max);
            }
            else
            {
                return (ascLists.Count, -1);
            }
        }


        /// <summary>
        /// 후방연속, 연속최대, 다음출리스트 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터 배열</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출리스트)</returns>
        public static (int realCount, int maxCount, List<int> nextList) RealMaxNextList(IEnumerable<int> ascCollection)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int[] ascArray = ascCollection.Where(x => x >= 0).ToArray();

            if (DistinctList(ascArray).Count >= 2)
            {
                int real = 0;
                int last = ascArray[^1];

                //후방연속
                foreach (int n in ascCollection.Reverse())
                {
                    if (last == n)
                        real++;
                    else
                        break;
                }

                //연속최대
                int[] ascRemind = ascArray[..^real];
                int max = 0, dup = 0;
                List<int> indxs = new();
                List<int> nexts = new();

                for (int i = 0; i < ascRemind.Length; i++)
                {
                    int n = ascRemind[i];

                    if (n == last)
                    {
                        dup++;
                        indxs.Add(i);
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                //최종 부분이 나온 경우 때문에 한번 더 검사
                if (dup > max)
                {
                    max = dup;
                }

                if (indxs.Any())
                {
                    var repeats = Enumerable.Repeat(last, real);
                    foreach (int i in indxs)
                    {
                        Range range = new(i, i + real);
                        int[] ints = ascArray[range];

                        if (ints.SequenceEqual(repeats))
                        {
                            nexts.Add(ascArray[i + real]);
                        }
                    }
                }

                return (real, max, nexts);
            }
            else
            {
                return (ascArray.Length, -1, new List<int>());
            }
        }

        /// <summary>
        /// 후방연속, 연속최대, 다음출리스트 (연속최대가 -1:전부동일, 0:끝출외 무출)
        /// </summary>
        /// <param name="ascCollection">오름차순 리스트</param>
        /// <param name="kiho">검사부호</param>
        /// <param name="number">검사번호</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출리스트)</returns>
        public static (int realCount, int maxCount, List<int> nextList) RealMaxNextList
                      (IEnumerable<int> ascCollection, Kiho kiho, int number)
        {

            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int[] ascArray = ascCollection.ToArray();

            if (DistinctList(ascArray).Count >= 2)
            {
                int real = 0;

                //후방연속
                for (int i = ascArray.Length - 1; i >= 0; i--)
                {
                    if (IsSameOperation(kiho, number, ascArray[i]))
                        real++;
                    else
                        break;
                }

                //연속최대
                int[] ascRemind = ascArray[..^real];
                int max = 0, dup = 0;
                var idxs = new List<int>();
                var nexts = new List<int>();

                for (int i = 0; i < ascRemind.Length; i++)
                {
                    int n = ascRemind[i];
                    if (IsSameOperation(kiho, number, n))
                    {
                        dup++;
                        idxs.Add(i);
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                if (idxs.Any())
                {
                    foreach (int i in idxs)
                    {
                        int sameCount = 0;

                        for (int j = 0; j < real; j++)
                        {
                            int a = i + j;

                            if (a < ascArray.Length)
                            {
                                int n = ascArray[a];

                                if (IsSameOperation(kiho, number, n))
                                    sameCount++;
                                else
                                    break;
                            }
                        }

                        if (sameCount == real)
                        {
                            int a = i + real;

                            if (a < ascArray.Length)
                            {
                                int n = ascArray[a];
                                nexts.Add(n);
                            }
                        }
                    }
                }

                return (real, max, nexts);
            }
            else
            {
                return (ascArray.Length, -1, new List<int>());
            }
        }

        /// <summary>
        /// 후방연속, 연속최대, 다음출리스트
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터 배열</param>
        /// <param name="patternCount">후방패턴 검사갯수 (1 이상)</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출리스트)</returns>
        public static (int realCount, int maxCount, List<int> nextList) RealMaxNextList(IEnumerable<int> ascCollection, int patternCount)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int[] array = ascCollection.Where(x => x >= 0).ToArray();

            if (DistinctList(array).Count >= 2)
            {
                if (patternCount == 1)
                {
                    var (realCount, maxCount) = RealMaxCount(ascCollection);
                    var finds = array[0..^1].Select((val, idx) => (val, idx)).Where(x => x.val == array[^1]).Select(x => x.idx + 1);
                    var list = finds.Select(x => array[x]).ToList();
                    return (realCount, maxCount, list);
                }
                else
                {
                    int[] lastlength = array[^patternCount..];
                    int[] revslength = Enumerable.Reverse(lastlength).ToArray();
                    int[] revarray = Enumerable.Reverse(array).ToArray();

                    //후방연속 찾기
                    int real = 0;
                    for (int i = 0; i < revarray.Length; i++)
                    {
                        Range range = new(i, patternCount + i);
                        int[] source = revarray[range];

                        if (source.SequenceEqual(revslength))
                            real++;
                        else
                            break;
                    }

                    //연속최대 찾기
                    int max = 0, dup = 0;
                    int subfirst = lastlength[0];
                    var findIndexs = array[0..^patternCount].Select((val, idx) => (val, idx)).Where(x => x.val == subfirst).Select(x => x.idx);

                    List<int> nextvals = new();
                    foreach (int findIndex in findIndexs)
                    {
                        for (int i = findIndex; i < array.Length - patternCount; i += patternCount)
                        {
                            Range range = new(i, patternCount + i);
                            int[] source = array[range];

                            if (source.SequenceEqual(lastlength))
                            {
                                int n = array[patternCount + i];
                                nextvals.Add(n);
                                dup++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (dup > max)
                        {
                            max = dup;
                        }
                        dup = 0;
                    }

                    if (dup > max)
                    {
                        max = dup;
                    }

                    return (real, max, nextvals);
                }
            }
            else
            {
                return (array.Length, -1, new List<int>());
            }
        }

        /// <summary>
        /// 후방연속, 연속최대, 다음출리스트 묶음
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터 배열</param>
        /// <param name="combineCount">후방패턴 묶음 갯수</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출리스트) 리스트</returns>
        public static List<(int realCount, int maxCount, List<int> nextLis)> RealMaxNextCombine(IEnumerable<int> ascCollection, int combineCount = 5)
        {
            var lists = new List<(int realCount, int maxCount, List<int> nextLis)>();

            for (int i = 1; i <= combineCount; i++)
            {
                var tpl = RealMaxNextList(ascCollection, i);
                lists.Add(tpl);
            }

            return lists;
        }


        /// <summary>
        /// 후방연속, 연속최대, 다음출 배열리스트
        /// </summary>
        /// <param name="ascCollections">오름차순 데이터 배열리스트</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출 배열리스트)</returns>
        public static (int realCount, int maxCount, List<int[]> nextList) RealMaxNextLists(IEnumerable<int[]> ascCollections)
        {
            if (!ascCollections?.Any() ?? false)
            {
                throw new Exception("배열리스트에 요소가 없음.");
            }

            var ascLists = ascCollections.ToList();

            if (DistinctList(ascLists).Count >= 2)
            {
                int real = 0;
                int[] last = ascLists[^1];

                //후방연속
                for (int i = ascLists.Count - 1; i >= 0; i--)
                {
                    if (last.SequenceEqual(ascLists[i]))
                        real++;
                    else
                        break;
                }

                //연속최대
                int[][] ascRemind = ascLists.ToArray()[..^real];
                int max = 0, dup = 0;
                List<int> indxs = new();
                List<int[]> nexts = new();

                for (int i = 0; i < ascRemind.Length; i++)
                {
                    int[] n = ascRemind[i];

                    if (n.SequenceEqual(last))
                    {
                        dup++;
                        indxs.Add(i);
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                if (indxs.Any())
                {
                    foreach (int i in indxs)
                    {
                        int sameCount = 0;

                        for (int j = 0; j < real; j++)
                        {
                            int a = i + j;
                            int[] n = ascLists[a];

                            if (n.SequenceEqual(last))
                                sameCount++;
                            else
                                break;
                        }

                        if (sameCount == real)
                        {
                            int a = i + real;

                            if (a < ascLists.Count)
                            {
                                int[] n = ascLists[a];
                                nexts.Add(n);
                            }
                        }
                    }
                }

                return (real, max, nexts);
            }
            else
            {
                return (ascLists.Count, -1, new List<int[]>());
            }
        }

        /// <summary>
        /// 후방연속, 연속최대, 다음출 배열리스트
        /// </summary>
        /// <param name="ascCollections">오름차순 데이터 배열리스트</param>
        /// <param name="johapIndex">인덱스배열</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출 배열리스트)</returns>
        public static (int realCount, int maxCount, List<int[]> nextList) RealMaxNextLists
                      (IEnumerable<int[]> ascCollections, int[] johapIndex)
        {
            if (!ascCollections?.Any() ?? false)
            {
                throw new Exception("배열리스트에 요소가 없음.");
            }

            var ascLists = ascCollections.Select(x => johapIndex.Select(y => x[y]).ToArray()).ToList();

            var tpl = RealMaxNextLists(ascLists);
            return tpl;
        }

        /// <summary>
        /// 후방연속, 연속최대, 다음출 배열리스트
        /// </summary>
        /// <param name="ascCollections">오름차순 데이터 배열리스트</param>
        /// <param name="tpl">튜플 리스트(검사부호, 검사번호)</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출 배열리스트)</returns>
        public static (int realCount, int maxCount, List<int[]> nextList) RealMaxNextLists
                      (IEnumerable<int[]> ascCollections, List<(Kiho kiho, int num)> tpl)
        {

            if (!ascCollections?.Any() ?? false)
            {
                throw new Exception("배열리스트에 요소가 없음.");
            }

            var ascLists = ascCollections.ToList();

            if (ascLists[0].Length != tpl.Count)
            {
                throw new Exception("데이터갯수와 조건갯수가 다름.");
            }

            if (DistinctList(ascLists).Count >= 2)
            {
                int real = 0;

                //후방연속
                for (int i = ascLists.Count - 1; i >= 0; i--)
                {
                    if (IsPassOfArray(ascLists[i], tpl))
                        real++;
                    else
                        break;
                }

                //연속최대
                int[][] ascRemind = ascLists.ToArray()[..^real];
                int max = 0, dup = 0;
                List<int> indxs = new();
                List<int[]> nexts = new();

                for (int i = 0; i < ascRemind.Length; i++)
                {
                    int[] n = ascRemind[i];

                    if (IsPassOfArray(n, tpl))
                    {
                        dup++;
                        indxs.Add(i);
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                if (indxs.Any())
                {
                    foreach (int i in indxs)
                    {
                        int sameCount = 0;

                        for (int j = 0; j < real; j++)
                        {
                            int a = i + j;
                            int[] n = ascLists[a];

                            if (IsPassOfArray(n, tpl))
                                sameCount++;
                            else
                                break;
                        }

                        if (sameCount == real)
                        {
                            int a = i + real;

                            if (a < ascLists.Count)
                            {
                                int[] n = ascLists[a];
                                nexts.Add(n);
                            }
                        }
                    }
                }

                return (real, max, nexts);
            }
            else
            {
                return (ascLists.Count, -1, new List<int[]>());
            }
        }

        /// <summary>
        /// 후방연속, 연속최대, 다음출 배열리스트
        /// </summary>
        /// <param name="ascCollections">오름차순 데이터 배열리스트</param>
        /// <param name="patternCount">후방패턴 검사갯수 (1 이상)</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 다음출 배열리스트)</returns>
        public static (int realCount, int maxCount, List<int[]> nextList) RealMaxNextLists
                      (IEnumerable<int[]> ascCollections, int patternCount)
        {
            if (!ascCollections?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            var ascLists = ascCollections.ToList();

            if (DistinctList(ascLists).Count >= 2)
            {
                var realpattern = ascLists.ToArray()[^patternCount..];
                var revspattern = realpattern.AsEnumerable().Reverse().ToArray();
                int real = 0;

                //후방연속
                for (int i = ascLists.Count - 1; i >= 0; i -= patternCount)
                {
                    int sameCount = 0;

                    for (int j = 0; j < patternCount; j++)
                    {
                        int a = i - j;
                        int[] n = ascLists[a];

                        if (revspattern[j].SequenceEqual(n))
                            sameCount++;
                        else
                            break;
                    }

                    if (sameCount == patternCount)
                        real++;
                    else
                        break;
                }

                int max = 0, dup = 0;
                List<int> indxs = new();
                List<int[]> nexts = new();
                int ix;

                //연속최대
                for (ix = 0; ix < ascLists.Count - patternCount; ix++)
                {
                    int sameCount = 0;
                    for (int j = 0; j < patternCount; j++)
                    {
                        int a = ix + j;
                        int[] n = ascLists[a];

                        if (realpattern[j].SequenceEqual(n))
                            sameCount++;
                        else
                            break;
                    }

                    if (sameCount == patternCount)
                    {
                        dup++;
                        indxs.Add(ix);
                        ix += patternCount - 1;
                    }
                    else
                    {
                        if (dup > max)
                        {
                            max = dup;
                        }

                        dup = 0;
                    }
                }

                if (dup > max)
                {
                    max = dup;
                }

                if (indxs.Any())
                {
                    foreach (int i in indxs)
                    {
                        int a = i + patternCount;

                        if (a < ascLists.Count)
                        {
                            int[] n = ascLists[a];
                            nexts.Add(n);
                        }
                    }
                }

                return (real, max, nexts);
            }
            else
            {
                return (ascLists.Count, -1, new List<int[]>());
            }
        }


        /// <summary>
        /// 후방연속, 연속최대, 동출수, 무출수, 유출수, 출수율, 다음출리스트 
        /// </summary>
        /// <param name="ascCollection">오름차순 데이터 배열</param>
        /// <returns>튜플(후방연속갯수, 연속최대갯수, 끝수동일수, 0출갯수, 1출이상갯수, 출수율, 다음출리스트)</returns>
        public static (int realCount, int maxCount, int sameCount, int nonCount, 
                       int showCount, double persent, List<int> nextList) MultipleRealMaxNextList(IEnumerable<int> ascCollection)
        {
            if (!ascCollection?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            int same = 0, non = 0, shown = 0;
            double pers = 0.00;

            if (ascCollection.Distinct().Count() >= 2)
            {
                int last = ascCollection.Last();
                var (realCount, maxCount, nextList) = RealMaxNextList(ascCollection);
                same = ascCollection.Count(x => x.Equals(last));

                if (ascCollection.Max() < 9 && DistinctList(ascCollection).Count < 9)
                {
                    non = ascCollection.Where(x => x > -1).Count(x => x == 0);
                    shown = ascCollection.Where(x => x > -1).Count() - non;

                    if (shown < 1)
                    {
                        pers = 0.00;
                    }
                    else
                    {
                        var di = shown * 100.0 / ascCollection.Where(x => x > -1).Count();
                        pers = Math.Round(di, 2);
                    }
                }

                return (realCount, maxCount, same, non, shown, pers, nextList);
            }
            else
            {
                return (ascCollection.Count(), -1, same, non, shown, pers, new List<int>());
            }
        }





        // 내부 메서드






        /// <summary>
        /// 쿼리문자중 컬럼부분 잘라내기
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static string[] SubstringQuery(string query)
        {
            string upper = query.ToUpper();
            int first = upper.IndexOf("SELECT");
            int last = upper.IndexOf("FROM");
            int length = last - 6 - first;
            string sub = query.Substring(first + 6, length).Trim();
            var split = sub.Split(new[] { "," }, StringSplitOptions.None);

            return split;
        }

        /// <summary>
        /// 배열리스트에서 고유요소 배열리스트 반환
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static List<int[]> DistinctList(IEnumerable<int[]> lists)
        {
            var addlists = new List<int[]>();

            if (!lists?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            foreach (int[] array in lists)
            {
                if (!addlists.Any())
                {
                    addlists.Add(array);
                }
                else
                {
                    if (addlists.All(x => !x.SequenceEqual(array)))
                    {
                        addlists.Add(array);
                    }
                }
            }

            List<int[]> ord = new();
            int length = addlists.First().Length;
            ord = length switch
            {
                1 => ord = addlists.OrderBy(x => x[0]).ToList(),
                2 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ToList(),
                3 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ToList(),
                4 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[3]).ToList(),
                5 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[3]).ThenBy(x => x[4]).ToList(),
                6 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[3]).ThenBy(x => x[4])
                                   .ThenBy(x => x[5]).ToList(),
                7 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[3]).ThenBy(x => x[4])
                                   .ThenBy(x => x[5]).ThenBy(x => x[6]).ToList(),
                8 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[3]).ThenBy(x => x[4])
                                   .ThenBy(x => x[5]).ThenBy(x => x[6]).ThenBy(x => x[7]).ToList(),
                9 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[3]).ThenBy(x => x[4])
                                   .ThenBy(x => x[5]).ThenBy(x => x[6]).ThenBy(x => x[7]).ThenBy(x => x[8]).ToList(),
                10 => ord = addlists.OrderBy(x => x[0]).ThenBy(x => x[1]).ThenBy(x => x[2]).ThenBy(x => x[3]).ThenBy(x => x[4])
                                   .ThenBy(x => x[5]).ThenBy(x => x[6]).ThenBy(x => x[7]).ThenBy(x => x[8]).ThenBy(x => x[9]).ToList(),
                _ => throw new Exception("정렬할 배열의 길이는 1 - 10 까지 입니다.")
            };

            return ord;
        }

        /// <summary>
        /// 리스트에서 고유요소 리스트 반환
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static List<int> DistinctList(IEnumerable<int> lists)
        {
            if (!lists?.Any() ?? false)
            {
                throw new Exception("배열에 요소가 없습니다.");
            }

            return lists.Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// 검사식에 적합한지 여부
        /// </summary>
        /// <param name="kiho">열거형부호</param>
        /// <param name="number">검사번호</param>
        /// <param name="inputNumber">입력번호</param>
        /// <returns>검사식에 적합하면 참</returns>
        private static bool IsSameOperation(Kiho kiho, int number, int inputNumber)
        {
            bool pass = false;

            switch (kiho)
            {
                case Kiho.Miman:
                    pass = (inputNumber < number);
                    break;
                case Kiho.Iha:
                    pass = (inputNumber <= number);
                    break;
                case Kiho.Gatum:
                    pass = (inputNumber == number);
                    break;
                case Kiho.Isang:
                    pass = (inputNumber >= number);
                    break;
                case Kiho.Choga:
                    pass = (inputNumber > number);
                    break;
                case Kiho.Darum:
                    pass = (inputNumber != number);
                    break;
                default:
                    break;
            }

            return pass;
        }

        /// <summary>
        /// 배열의 검사식 통과여부
        /// </summary>
        /// <param name="arrays">배열</param>
        /// <param name="tpl">튜플(검사부호, 검사번호)</param>
        /// <returns>검사통과하면 참</returns>
        private static bool IsPassOfArray(int[] arrays, List<(Kiho kiho, int num)> tpl)
        {
            if (arrays.Length != tpl.Count)
            {
                throw new Exception("조건식과 배열의 길이가 다름.");
            }

            int error = 0;

            for (int i = 0; i < arrays.Length; i++)
            {
                var (kiho, num) = tpl[i];
                bool sub = IsSameOperation(kiho, num, arrays[i]);

                if (!sub)
                {
                    error++;
                    break;
                }
            }

            return (error == 0);
        }

    }
}
