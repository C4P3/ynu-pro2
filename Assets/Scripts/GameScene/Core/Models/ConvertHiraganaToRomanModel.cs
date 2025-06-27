using System.Collections.Generic;

namespace Models
{
    public class ConvertHiraganaToRomanModel
    {
        public IEnumerable<char> ConvertHiraganaToRoman(IReadOnlyList<char> hiragana)
        {
            var roman = new List<char>();

            for (var i = 0; i < hiragana.Count; i++) 
            {
                var nextHiragana = i < hiragana.Count - 1 ? hiragana[i + 1] : '@';
                switch (hiragana[i])
                {
                    case 'あ':
                    {
                        roman.Add('a');
                        break;
                    }
                    case 'い':
                    {
                        roman.Add('i');
                        break;
                    }
                    case 'う':
                    {
                        switch (nextHiragana)
                        {
                            case 'ぁ':
                            {
                                roman.Add('w');
                                roman.Add('h');
                                roman.Add('a');
                                break;
                            }
                            case 'ぃ':
                            {
                                roman.Add('w');
                                roman.Add('h');
                                roman.Add('i');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('w');
                                roman.Add('h');
                                roman.Add('e');
                                break;
                            }
                            case 'ぉ':
                            {
                                roman.Add('w');
                                roman.Add('h');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('u');
                                break;
                            }
                        }
                        break;
                    }
                    case 'え':
                    {
                        roman.Add('e');
                        break;
                    }
                    case 'お':
                    {
                        roman.Add('o');
                        break;
                    }
                    case 'か':
                    {
                        roman.Add('k');
                        roman.Add('a');
                        break;
                    }
                    case 'き':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('k');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('k');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('k');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('k');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('k');
                                roman.Add('i');
                                break;
                            }
                        }

                        break;
                    }
                    case 'く':
                    {
                        roman.Add('k');
                        roman.Add('u');
                        break;
                    }
                    case 'け':
                    {
                        roman.Add('k');
                        roman.Add('e');
                        break;
                    }
                    case 'こ':
                    {
                        roman.Add('k');
                        roman.Add('o');
                        break;
                    }
                    case 'さ':
                    {
                        roman.Add('s');
                        roman.Add('a');
                        break;
                    }
                    case 'し':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('s');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('s');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('s');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('s');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('s');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'す':
                    {
                        roman.Add('s');
                        roman.Add('u');
                        break;
                    }
                    case 'せ':
                    {
                        roman.Add('s');
                        roman.Add('e');
                        break;
                    }
                    case 'そ':
                    {
                        roman.Add('s');
                        roman.Add('o');
                        break;
                    }
                    case 'た':
                    {
                        roman.Add('t');
                        roman.Add('a');
                        break;
                    }
                    case 'ち':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('t');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('t');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('t');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('t');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('t');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'つ':
                    {
                        switch (nextHiragana)
                        {
                            case 'ぁ':
                            {
                                roman.Add('t');
                                roman.Add('s');
                                roman.Add('a');
                                break;
                            }
                            case 'ぃ':
                            {
                                roman.Add('t');
                                roman.Add('s');
                                roman.Add('i');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('t');
                                roman.Add('s');
                                roman.Add('e');
                                break;
                            }
                            case 'ぉ':
                            {
                                roman.Add('t');
                                roman.Add('s');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('t');
                                roman.Add('u');
                                break;
                            }
                        }

                        break;
                    }
                    case 'て':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('t');
                                roman.Add('h');
                                roman.Add('a');
                                break;
                            }
                            case 'ぃ':
                            {
                                roman.Add('t');
                                roman.Add('h');
                                roman.Add('i');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('t');
                                roman.Add('h');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('t');
                                roman.Add('h');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('t');
                                roman.Add('h');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('t');
                                roman.Add('e');
                                break;
                            }
                        }

                        break;
                    }
                    case 'と':
                    {
                        switch (nextHiragana)
                        {
                            case 'ぁ':
                            {
                                roman.Add('t');
                                roman.Add('w');
                                roman.Add('a');
                                break;
                            }
                            case 'ぃ':
                            {
                                roman.Add('t');
                                roman.Add('w');
                                roman.Add('i');
                                break;
                            }
                            case 'ぅ':
                            {
                                roman.Add('t');
                                roman.Add('w');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('t');
                                roman.Add('w');
                                roman.Add('e');
                                break;
                            }
                            case 'ぉ':
                            {
                                roman.Add('t');
                                roman.Add('w');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('t');
                                roman.Add('o');
                                break;
                            }
                        }

                        break;
                    }
                    case 'な':
                    {
                        roman.Add('n');
                        roman.Add('a');
                        break;
                    }
                    case 'に':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('n');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('n');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('n');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('n');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('n');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ぬ':
                    {
                        roman.Add('n');
                        roman.Add('u');
                        break;
                    }
                    case 'ね':
                    {
                        roman.Add('n');
                        roman.Add('e');
                        break;
                    }
                    case 'の':
                    {
                        roman.Add('n');
                        roman.Add('o');
                        break;
                    }
                    case 'は':
                    {
                        roman.Add('h');
                        roman.Add('a');
                        break;
                    }
                    case 'ひ':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('h');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('h');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('h');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('h');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('n');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ふ':
                    {
                        switch (nextHiragana)
                        {
                            case 'ぁ':
                            {
                                roman.Add('f');
                                roman.Add('a');
                                break;
                            }
                            case 'ぃ':
                            {
                                roman.Add('f');
                                roman.Add('i');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('f');
                                roman.Add('e');
                                break;
                            }
                            case 'ぉ':
                            {
                                roman.Add('f');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('h');
                                roman.Add('u');
                                break;
                            }
                        }
                        break;
                    }
                    case 'へ':
                    {
                        roman.Add('h');
                        roman.Add('e');
                        break;
                    }
                    case 'ほ':
                    {
                        roman.Add('h');
                        roman.Add('o');
                        break;
                    }
                    case 'ま':
                    {
                        roman.Add('m');
                        roman.Add('a');
                        break;
                    }
                    case 'み':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('m');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('m');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('m');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('m');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('m');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'む':
                    {
                        roman.Add('m');
                        roman.Add('u');
                        break;
                    }
                    case 'め':
                    {
                        roman.Add('m');
                        roman.Add('e');
                        break;
                    }
                    case 'も':
                    {
                        roman.Add('m');
                        roman.Add('o');
                        break;
                    }
                    case 'や':
                    {
                        roman.Add('y');
                        roman.Add('a');
                        break;
                    }
                    case 'ゆ':
                    {
                        roman.Add('y');
                        roman.Add('u');
                        break;
                    }
                    case 'よ':
                    {
                        roman.Add('y');
                        roman.Add('o');
                        break;
                    }
                    case 'ら':
                    {
                        roman.Add('r');
                        roman.Add('a');
                        break;
                    }
                    case 'り':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('r');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('r');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('r');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('r');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('r');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'る':
                    {
                        roman.Add('r');
                        roman.Add('u');
                        break;
                    }
                    case 'れ':
                    {
                        roman.Add('r');
                        roman.Add('e');
                        break;
                    }
                    case 'ろ':
                    {
                        roman.Add('r');
                        roman.Add('o');
                        break;
                    }
                    case 'わ':
                    {
                        roman.Add('w');
                        roman.Add('a');
                        break;
                    }
                    case 'を':
                    {
                        roman.Add('w');
                        roman.Add('o');
                        break;
                    }
                    case 'ん':
                    {
                        var hashSet = new HashSet<char>
                        {
                            'あ', 'い', 'う', 'え', 'お', 'な', 'に', 'ぬ', 'ね', 'の', 'や', 'ゆ', 'よ', 'ん', '@'
                        };

                        if (hashSet.Contains(nextHiragana))
                        {
                            roman.Add('n');
                            roman.Add('n');
                        }
                        else
                        {
                            roman.Add('n');
                        }
                        break;
                    }
                    case 'が':
                    {
                        roman.Add('g');
                        roman.Add('a');
                        break;
                    }
                    case 'ぎ':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('g');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('g');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('g');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('g');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('g');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ぐ':
                    {
                        roman.Add('g');
                        roman.Add('u');
                        break;
                    }
                    case 'げ':
                    {
                        roman.Add('g');
                        roman.Add('e');
                        break;
                    }
                    case 'ご':
                    {
                        roman.Add('g');
                        roman.Add('o');
                        break;
                    }
                    case 'ざ':
                    {
                        roman.Add('z');
                        roman.Add('a');
                        break;
                    }
                    case 'じ':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('z');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('z');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('z');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('z');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('z');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ず':
                    {
                        roman.Add('z');
                        roman.Add('u');
                        break;
                    }
                    case 'ぜ':
                    {
                        roman.Add('z');
                        roman.Add('e');
                        break;
                    }
                    case 'ぞ':
                    {
                        roman.Add('z');
                        roman.Add('o');
                        break;
                    }
                    case 'だ':
                    {
                        roman.Add('d');
                        roman.Add('a');
                        break;
                    }
                    case 'ぢ':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('d');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('d');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('d');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('d');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('d');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'づ':
                    {
                        roman.Add('d');
                        roman.Add('u');
                        break;
                    }
                    case 'で':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('d');
                                roman.Add('h');
                                roman.Add('a');
                                break;
                            }
                            case 'ぃ':
                            {
                                roman.Add('d');
                                roman.Add('h');
                                roman.Add('i');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('d');
                                roman.Add('h');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('d');
                                roman.Add('h');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('d');
                                roman.Add('h');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('d');
                                roman.Add('e');
                                break;
                            }
                        }

                        break;
                    }
                    case 'ど':
                    {
                        switch (nextHiragana)
                        {
                            case 'ぁ':
                            {
                                roman.Add('d');
                                roman.Add('w');
                                roman.Add('a');
                                break;
                            }
                            case 'ぃ':
                            {
                                roman.Add('d');
                                roman.Add('w');
                                roman.Add('i');
                                break;
                            }
                            case 'ぅ':
                            {
                                roman.Add('d');
                                roman.Add('w');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('d');
                                roman.Add('w');
                                roman.Add('e');
                                break;
                            }
                            case 'ぉ':
                            {
                                roman.Add('d');
                                roman.Add('w');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('d');
                                roman.Add('o');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ば':
                    {
                        roman.Add('b');
                        roman.Add('a');
                        break;
                    }
                    case 'び':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('b');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('b');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('b');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('b');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('b');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ぶ':
                    {
                        roman.Add('b');
                        roman.Add('u');
                        break;
                    }
                    case 'べ':
                    {
                        roman.Add('b');
                        roman.Add('e');
                        break;
                    }
                    case 'ぼ':
                    {
                        roman.Add('b');
                        roman.Add('o');
                        break;
                    }
                    case 'ぱ':
                    {
                        roman.Add('p');
                        roman.Add('a');
                        break;
                    }
                    case 'ぴ':
                    {
                        switch (nextHiragana)
                        {
                            case 'ゃ':
                            {
                                roman.Add('p');
                                roman.Add('y');
                                roman.Add('a');
                                break;
                            }
                            case 'ゅ':
                            {
                                roman.Add('p');
                                roman.Add('y');
                                roman.Add('u');
                                break;
                            }
                            case 'ぇ':
                            {
                                roman.Add('p');
                                roman.Add('y');
                                roman.Add('e');
                                break;
                            }
                            case 'ょ':
                            {
                                roman.Add('p');
                                roman.Add('y');
                                roman.Add('o');
                                break;
                            }
                            default:
                            {
                                roman.Add('p');
                                roman.Add('i');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ぷ':
                    {
                        roman.Add('p');
                        roman.Add('u');
                        break;
                    }
                    case 'ぺ':
                    {
                        roman.Add('p');
                        roman.Add('e');
                        break;
                    }
                    case 'ぽ':
                    {
                        roman.Add('p');
                        roman.Add('o');
                        break;
                    }
                    case 'っ':
                    {
                        switch (nextHiragana)
                        {
                            case 'か' or 'き' or 'く' or 'け' or 'こ':
                            {
                                roman.Add('k');
                                break;
                            }
                            case 'さ' or 'し' or 'す' or 'せ' or 'そ':
                            {
                                roman.Add('s');
                                break;
                            }
                            case 'た' or 'ち' or 'つ' or 'て' or 'と':
                            {
                                roman.Add('t');
                                break;
                            }
                            case 'は' or 'ひ' or 'ふ' or 'へ' or 'ほ':
                            {
                                roman.Add('h');
                                break;
                            }
                            case 'ま' or 'み' or 'む' or 'め' or 'も':
                            {
                                roman.Add('m');
                                break;
                            }
                            case 'や' or 'ゆ' or 'よ':
                            {
                                roman.Add('y');
                                break;
                            }
                            case 'が' or 'ぎ' or 'ぐ' or 'げ' or 'ご':
                            {
                                roman.Add('g');
                                break;
                            }
                            case 'ざ' or 'じ' or 'ず' or 'ぜ' or 'ぞ':
                            {
                                roman.Add('z');
                                break;
                            }
                            case 'だ' or 'ぢ' or 'づ' or 'で' or 'ど':
                            {
                                roman.Add('d');
                                break;
                            }
                            case 'ば' or 'び' or 'ぶ' or 'べ' or 'ぼ':
                            {
                                roman.Add('b');
                                break;
                            }
                            case 'ぱ' or 'ぴ' or 'ぷ' or 'ぺ' or 'ぽ':
                            {
                                roman.Add('p');
                                break;
                            }
                            default:
                            {
                                roman.Add('x');
                                roman.Add('t');
                                roman.Add('u');
                                break;
                            }
                        }
                        break;
                    }
                    case 'ー':
                    {
                        roman.Add('-');
                        break;
                    }
                    case '、':
                    {
                        roman.Add(',');
                        break;
                    }
                    case '。':
                    {
                        roman.Add('.');
                        break;
                    }
                }
            }

            return roman;
        }
    }
}