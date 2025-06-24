using System;
using System.Collections.Generic;

namespace Models
{
    public enum OperatingSystemName { None, Windows, Mac }

    public enum TypeResult
    {
        Correct,    // 正しい入力
        Incorrect,  // 間違った入力
        Finished    // 全て正しく入力し終えた
    }

    public class CurrentTypingTextModel
    {
        private string _title = "";
        private List<char> _characters = new List<char>();
        private int _charactersIndex = 0;
        private OperatingSystemName _operatingSystemName = OperatingSystemName.None;

        public void SetOperatingSystemName(OperatingSystemName operatingSystemName)
        {
            _operatingSystemName = operatingSystemName;
        }

        public void SetTitle(string title)
        {
            _title = title;
        }

        public void SetCharacters(IEnumerable<char> characters)
        {
            _characters = new List<char>(characters);
        }

        public void ResetCharactersIndex()
        {
            _charactersIndex = 0;
        }
        
        public TypeResult TypeCharacter(char inputCharacter)
        {
            var currentCharactersIndex = _charactersIndex;
            var currentCharacters = new List<char>(_characters);
            
            var prevChar3 = currentCharactersIndex >= 3 ? currentCharacters[currentCharactersIndex - 3] : '\0';
            var prevChar2 = currentCharactersIndex >= 2 ? currentCharacters[currentCharactersIndex - 2] : '\0';
            var prevChar = currentCharactersIndex >= 1 ? currentCharacters[currentCharactersIndex - 1] : '\0';
            var currentChar = currentCharacters[currentCharactersIndex];
            var nextChar = currentCharactersIndex < currentCharacters.Count - 1
                ? currentCharacters[currentCharactersIndex + 1]
                : '\0';
            var nextChar2 = currentCharactersIndex < currentCharacters.Count - 2
                ? currentCharacters[currentCharactersIndex + 2]
                : '\0';

            var newCharacters = new List<char>(currentCharacters);

            var isCorrect = false;

            if (inputCharacter == newCharacters[currentCharactersIndex])
            {
                isCorrect = true;
            }

            //「い」の入力判定（Windowsのみ）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter is 'y' &&
                     currentChar is 'i' &&
                     prevChar is '\0' or 'a' or 'i' or 'u' or 'e' or 'o')
            {
                newCharacters.Insert(currentCharactersIndex, 'y');
                isCorrect = true;
            }

            //「う」の入力判定（「whu」はWindowsのみ）
            else if (inputCharacter is 'w' &&
                     currentChar is 'u' &&
                     prevChar is '\0' or 'a' or 'i' or 'u' or 'e' or 'o')
            {
                newCharacters.Insert(currentCharactersIndex, 'w');
                isCorrect = true;
            }

            else if (inputCharacter == 'w' &&
                     currentChar == 'u' &&
                     prevChar == 'n' &&
                     prevChar2 == 'n' &&
                     prevChar3 != 'n')
            {
                newCharacters.Insert(currentCharactersIndex, 'w');
                isCorrect = true;
            }

            else if (inputCharacter == 'w' &&
                     currentChar == 'u' &&
                     prevChar == 'n' &&
                     prevChar2 == 'x')
            {
                newCharacters.Insert(currentCharactersIndex, 'w');
                isCorrect = true;
            }

            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'h' &&
                     currentChar == 'u' &&
                     prevChar2 != 't' && prevChar2 != 'd' &&
                     prevChar == 'w')
            {
                newCharacters.Insert(currentCharactersIndex, 'h');
                isCorrect = true;
            }

            //「か」「く」「こ」の柔軟な入力（Windowsのみ）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'c' &&
                     prevChar != 'k' &&
                     currentChar == 'k' && nextChar is 'a' or 'u' or 'o')
            {
                newCharacters[currentCharactersIndex] = 'c';
                isCorrect = true;
            }

            //「く」の柔軟な入力（Windowsのみ）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'q' &&
                     prevChar != 'k' &&
                     currentChar == 'k' && nextChar == 'u')
            {
                newCharacters[currentCharactersIndex] = 'q';
                isCorrect = true;
            }

            //「し」の柔軟な入力
            else if (inputCharacter == 'h' && prevChar == 's' && currentChar == 'i')
            {
                newCharacters.Insert(currentCharactersIndex, 'h');
                isCorrect = true;
            }

            //「じ」の柔軟な入力
            else if (inputCharacter == 'j' && currentChar == 'z' && nextChar == 'i')
            {
                newCharacters[currentCharactersIndex] = 'j';
                isCorrect = true;
            }

            //「しゃ」「しゅ」「しぇ」「しょ」の柔軟な入力
            else if (inputCharacter == 'h' && prevChar == 's' && currentChar == 'y')
            {
                newCharacters[currentCharactersIndex] = 'h';
                isCorrect = true;
            }

            //「じゃ」「じゅ」「じぇ」「じょ」の柔軟な入力
            else if (inputCharacter == 'j' && prevChar != 'z' && currentChar == 'z' &&
                     nextChar == 'y' && nextChar2 is 'a' or 'u' or 'e' or 'o')
            {
                newCharacters[currentCharactersIndex] = 'j';
                newCharacters.RemoveAt(currentCharactersIndex + 1);
                isCorrect = true;
            }

            else if (inputCharacter == 'y' && prevChar == 'j' &&
                     currentChar is 'a' or 'u' or 'e' or 'o')
            {
                newCharacters.Insert(currentCharactersIndex, 'y');
                isCorrect = true;
            }

            //「し」「せ」の柔軟な入力（Windowsのみ）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'c' && prevChar != 's' &&
                     currentChar == 's' &&
                     nextChar is 'i' or 'e')
            {
                newCharacters[currentCharactersIndex] = 'c';
                isCorrect = true;
            }

            //「ち」の柔軟な入力
            else if (inputCharacter == 'c' && prevChar != 't' && currentChar == 't' && nextChar == 'i')
            {
                newCharacters[currentCharactersIndex] = 'c';
                newCharacters.Insert(currentCharactersIndex + 1, 'h');
                isCorrect = true;
            }

            //「ちゃ」「ちゅ」「ちぇ」「ちょ」の柔軟な入力
            else if (inputCharacter == 'c' && prevChar != 't' && currentChar == 't' && nextChar == 'y')
            {
                newCharacters[currentCharactersIndex] = 'c';
                isCorrect = true;
            }

            //「cya」=>「cha」
            else if (inputCharacter == 'h' && prevChar == 'c' && currentChar == 'y')
            {
                newCharacters[currentCharactersIndex] = 'h';
                isCorrect = true;
            }

            //「つ」の柔軟な入力
            else if (inputCharacter == 's' && prevChar == 't' && currentChar == 'u')
            {
                newCharacters.Insert(currentCharactersIndex, 's');
                isCorrect = true;
            }

            //「つぁ」「つぃ」「つぇ」「つぉ」の柔軟な入力
            else if (inputCharacter == 'u' && prevChar == 't' && currentChar == 's' &&
                     nextChar is 'a' or 'i' or 'e' or 'o')
            {
                newCharacters[currentCharactersIndex] = 'u';
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            else if (inputCharacter == 'u' && prevChar2 == 't' && prevChar == 's' &&
                     currentChar is 'a' or 'i' or 'e' or 'o')
            {
                newCharacters.Insert(currentCharactersIndex, 'u');
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            //「てぃ」の柔軟な入力
            else if (inputCharacter == 'e' && prevChar == 't' && currentChar == 'h' && nextChar == 'i')
            {
                newCharacters[currentCharactersIndex] = 'e';
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            //「でぃ」の柔軟な入力
            else if (inputCharacter == 'e' && prevChar == 'd' && currentChar == 'h' && nextChar == 'i')
            {
                newCharacters[currentCharactersIndex] = 'e';
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            //「でゅ」の柔軟な入力
            else if (inputCharacter == 'e' && prevChar == 'd' && currentChar == 'h' && nextChar == 'u')
            {
                newCharacters[currentCharactersIndex] = 'e';
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                newCharacters.Insert(currentCharactersIndex + 2, 'y');
                isCorrect = true;
            }

            //「とぅ」の柔軟な入力
            else if (inputCharacter == 'o' && prevChar == 't' && currentChar == 'w' && nextChar == 'u')
            {
                newCharacters[currentCharactersIndex] = 'o';
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            //「どぅ」の柔軟な入力
            else if (inputCharacter == 'o' && prevChar == 'd' && currentChar == 'w' && nextChar == 'u')
            {
                newCharacters[currentCharactersIndex] = 'o';
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            //「ふ」の柔軟な入力
            else if (inputCharacter == 'f' && currentChar == 'h' && nextChar == 'u')
            {
                newCharacters[currentCharactersIndex] = 'f';
                isCorrect = true;
            }

            //「ふぁ」「ふぃ」「ふぇ」「ふぉ」の柔軟な入力（一部Macのみ）
            else if (inputCharacter == 'w' && prevChar == 'f' &&
                     currentChar is 'a' or 'i' or 'e' or 'o')
            {
                newCharacters.Insert(currentCharactersIndex, 'w');
                isCorrect = true;
            }

            else if (inputCharacter == 'y' && prevChar == 'f' && currentChar is 'i' or 'e')
            {
                newCharacters.Insert(currentCharactersIndex, 'y');
                isCorrect = true;
            }

            else if (inputCharacter == 'h' && prevChar != 'f' && currentChar == 'f' &&
                     nextChar is 'a' or 'i' or 'e' or 'o')
            {
                if (_operatingSystemName == OperatingSystemName.Mac)
                {
                    newCharacters[currentCharactersIndex] = 'h';
                    newCharacters.Insert(currentCharactersIndex + 1, 'w');
                }
                else
                {
                    newCharacters[currentCharactersIndex] = 'h';
                    newCharacters.Insert(currentCharactersIndex + 1, 'u');
                    newCharacters.Insert(currentCharactersIndex + 2, 'x');
                }

                isCorrect = true;
            }

            else if (inputCharacter == 'u' && prevChar == 'f' &&
                     currentChar is 'a' or 'i' or 'e' or 'o')
            {
                newCharacters.Insert(currentCharactersIndex, 'u');
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            else if (_operatingSystemName == OperatingSystemName.Mac && inputCharacter == 'u' &&
                     prevChar == 'h' &&
                     currentChar == 'w' &&
                     nextChar is 'a' or 'i' or 'e' or 'o')
            {
                newCharacters[currentCharactersIndex] = 'u';
                newCharacters.Insert(currentCharactersIndex + 1, 'x');
                isCorrect = true;
            }

            //「ん」の柔軟な入力（「n'」には未対応）
            else if (inputCharacter == 'n' && prevChar2 != 'n' && prevChar == 'n' && currentChar != 'a' &&
                     currentChar != 'i' &&
                     currentChar != 'u' && currentChar != 'e' && currentChar != 'o' && currentChar != 'y')
            {
                newCharacters.Insert(currentCharactersIndex, 'n');
                isCorrect = true;
            }

            else if (inputCharacter == 'x' && prevChar != 'n' && currentChar == 'n' && nextChar != 'a' &&
                     nextChar != 'i' &&
                     nextChar != 'u' && nextChar != 'e' && nextChar != 'o' && nextChar != 'y')
            {
                if (nextChar == 'n')
                {
                    newCharacters[currentCharactersIndex] = 'x';
                }
                else
                {
                    newCharacters.Insert(currentCharactersIndex, 'x');
                }

                isCorrect = true;
            }

            //「うぃ」「うぇ」「うぉ」を分解する
            else if (inputCharacter == 'u' && currentChar == 'w' && nextChar == 'h' &&
                     nextChar2 is 'a' or 'i' or 'e' or 'o')
            {
                newCharacters[currentCharactersIndex] = 'u';
                newCharacters[currentCharactersIndex + 1] = 'x';
                isCorrect = true;
            }

            //「きゃ」「にゃ」などを分解する
            else if (inputCharacter == 'i' && currentChar == 'y' &&
                     prevChar is 'k' or 's' or 't' or 'n' or 'h' or 'm' or 'r' or 'g' or 'z' or 'd' or 'b' or 'p' &&
                     nextChar is 'a' or 'u' or 'e' or 'o')
            {
                if (nextChar == 'e')
                {
                    newCharacters[currentCharactersIndex] = 'i';
                    newCharacters.Insert(currentCharactersIndex + 1, 'x');
                }
                else
                {
                    newCharacters.Insert(currentCharactersIndex, 'i');
                    newCharacters.Insert(currentCharactersIndex + 1, 'x');
                }

                isCorrect = true;
            }

            //「しゃ」「ちゃ」などを分解する
            else if (inputCharacter == 'i' &&
                     currentChar is 'a' or 'u' or 'e' or 'o' &&
                     prevChar2 is 's' or 'c' && prevChar == 'h')
            {
                if (nextChar == 'e')
                {
                    newCharacters.Insert(currentCharactersIndex, 'i');
                    newCharacters.Insert(currentCharactersIndex + 1, 'x');
                }
                else
                {
                    newCharacters.Insert(currentCharactersIndex, 'i');
                    newCharacters.Insert(currentCharactersIndex + 1, 'x');
                    newCharacters.Insert(currentCharactersIndex + 2, 'y');
                }

                isCorrect = true;
            }

            //「しゃ」を「c」で分解する（Windows限定）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'c' &&
                     currentChar == 's' &&
                     prevChar != 's' && nextChar == 'y' &&
                     nextChar2 is 'a' or 'u' or 'e' or 'o')
            {
                if (nextChar2 == 'e')
                {
                    newCharacters[currentCharactersIndex] = 'c';
                    newCharacters[currentCharactersIndex + 1] = 'i';
                    newCharacters.Insert(currentCharactersIndex + 1, 'x');
                }
                else
                {
                    newCharacters[currentCharactersIndex] = 'c';
                    newCharacters.Insert(currentCharactersIndex + 1, 'i');
                    newCharacters.Insert(currentCharactersIndex + 2, 'x');
                }

                isCorrect = true;
            }

            //「っ」の柔軟な入力
            else if (inputCharacter is 'x' or 'l' &&
                     (currentChar == 'k' && nextChar == 'k' || currentChar == 's' && nextChar == 's' ||
                      currentChar == 't' && nextChar == 't' || currentChar == 'g' && nextChar == 'g' ||
                      currentChar == 'z' && nextChar == 'z' || currentChar == 'j' && nextChar == 'j' ||
                      currentChar == 'd' && nextChar == 'd' || currentChar == 'b' && nextChar == 'b' ||
                      currentChar == 'p' && nextChar == 'p'))
            {
                newCharacters[currentCharactersIndex] = inputCharacter;
                newCharacters.Insert(currentCharactersIndex + 1, 't');
                newCharacters.Insert(currentCharactersIndex + 2, 'u');
                isCorrect = true;
            }

            //「っか」「っく」「っこ」の柔軟な入力（Windows限定）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'c' && currentChar == 'k' &&
                     nextChar == 'k' &&
                     nextChar2 is 'a' or 'u' or 'o')
            {
                newCharacters[currentCharactersIndex] = 'c';
                newCharacters[currentCharactersIndex + 1] = 'c';
                isCorrect = true;
            }

            //「っく」の柔軟な入力（Windows限定）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'q' && currentChar == 'k' &&
                     nextChar == 'k' && nextChar2 == 'u')
            {
                newCharacters[currentCharactersIndex] = 'q';
                newCharacters[currentCharactersIndex + 1] = 'q';
                isCorrect = true;
            }

            //「っし」「っせ」の柔軟な入力（Windows限定）
            else if (_operatingSystemName == OperatingSystemName.Windows &&
                     inputCharacter == 'c' && currentChar == 's' &&
                     nextChar == 's' &&
                     nextChar2 is 'i' or 'e')
            {
                newCharacters[currentCharactersIndex] = 'c';
                newCharacters[currentCharactersIndex + 1] = 'c';
                isCorrect = true;
            }

            //「っちゃ」「っちゅ」「っちぇ」「っちょ」の柔軟な入力
            else if (inputCharacter == 'c' && currentChar == 't' && nextChar == 't' && nextChar2 == 'y')
            {
                newCharacters[currentCharactersIndex] = 'c';
                newCharacters[currentCharactersIndex + 1] = 'c';
                isCorrect = true;
            }

            //「っち」の柔軟な入力
            else if (inputCharacter == 'c' && currentChar == 't' && nextChar == 't' && nextChar2 == 'i')
            {
                newCharacters[currentCharactersIndex] = 'c';
                newCharacters[currentCharactersIndex + 1] = 'c';
                newCharacters.Insert(currentCharactersIndex + 2, 'h');
                isCorrect = true;
            }

            //「l」と「x」の完全互換性
            else if (inputCharacter == 'x' && currentChar == 'l')
            {
                newCharacters[currentCharactersIndex] = 'x';
                isCorrect = true;
            }

            else if (inputCharacter == 'l' && currentChar == 'x')
            {
                newCharacters[currentCharactersIndex] = 'l';
                isCorrect = true;
            }


            if (isCorrect)
            {
                _characters = newCharacters;
                _charactersIndex++;

                if (_charactersIndex >= _characters.Count)
                {
                    return TypeResult.Finished;
                }

                return TypeResult.Correct;
            }

            return TypeResult.Incorrect;
        }
    }
}