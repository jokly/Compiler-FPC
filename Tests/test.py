import os
import argparse
import glob
import subprocess as subpr
from os.path import basename

TESTS_PATH = os.path.dirname(os.path.realpath(__file__)) + os.sep

PATH_TO_COMPILER = TESTS_PATH + '..' + os.sep + 'Compiler-FPC' + os.sep + 'bin' + \
    os.sep + 'Debug' + os.sep + 'Compiler-FPC.exe'

TESTS = {
    '-l': [TESTS_PATH +'Lexer'],
    '-p': [TESTS_PATH +'Parser'],
    '-s': [TESTS_PATH +'Semantic'],
}

GEN_TESTS = {
    '-g': [TESTS_PATH + 'AsmGenerator']
}

def gen_test(folders, optinons=[]):
    for folder in folders:
        print(folder + '...')

        inputFiles = getInputFiles(folder)
        outputFiles = getOutputFiles(folder)
        answerFiles = getAnswerFile(folder)

        countTests = 0
        for file in inputFiles:
            fileName = os.path.splitext(file)[0]
            asmFileName = fileName + '.asm'

            subpr.run([PATH_TO_COMPILER, '-f', file, '-o', asmFileName] + optinons)

            if open(asmFileName, 'r').read()[0] == '(':
                if open(asmFileName, 'r').read() != open(fileName + '.ans').read():
                    print('Passed (' + str(countTests) + os.sep + str(len(inputFiles)) + ')')
                    print('Not passed: ' + asmFileName)
                    exit(0)
                else:
                    print(asmFileName + '...OK')
                    countTests += 1
                    continue

            subpr.run(['nasm', '-f', 'win32', asmFileName, '-o', fileName + '.obj'])
            subpr.run(['gcc', '-m32', fileName + '.obj', '-o', fileName + '.exe'])
            result = subpr.run([os.path.splitext(file)[0]], stdout=subpr.PIPE)

            outFileName = fileName + '.out'
            open(outFileName, 'w').write(result.stdout.decode('utf-8'))

            ansFileName = fileName + '.ans'
            if result.stdout.decode('utf-8') != open(ansFileName, 'r').read():
                print('Passed (' + str(countTests) + os.sep + str(len(inputFiles)) + ')')
                print('Not passed: ' + outFileName)
                exit(0)
            else:
                print(outFileName + '...OK')

            countTests += 1

        print('Passed (' + str(countTests) + '/' + str(len(inputFiles)) + ')')

    print('All tests passed!')     

def test(folders, optinons = []):
    for folder in folders:
        print(folder + '...')

        inputFiles = getInputFiles(folder)
        outputFiles = getOutputFiles(folder)
        answerFiles = getAnswerFile(folder)

        countTests = 0
        for file in inputFiles:
            outFileName = os.path.splitext(file)[0] + '.out'
            subpr.run([PATH_TO_COMPILER, '-f', file, '-o', outFileName] + optinons)

            ansFileName = os.path.splitext(file)[0] + '.ans'
            if open(outFileName, 'r').read() != open(ansFileName, 'r').read():
                print('Passed (' + str(countTests) + os.sep + str(len(inputFiles)) + ')')
                print('Not passed: ' + outFileName)
                exit(0)
            else:
                print(outFileName + '...OK')

            countTests += 1

        print('Passed (' + str(countTests) + '/' + str(len(inputFiles)) + ')')

    print('All tests passed!') 

def getInputFiles(folder):
    return glob.glob(folder + os.sep + '*.in')

def getOutputFiles(folder):
    return glob.glob(folder + os.sep + '*.out')

def getAnswerFile(folder):
    return glob.glob(folder + os.sep + '*.ans')

def getAllDirs(testsPath):
    allDirs = []

    for root, dirs, files in os.walk(testsPath):
        allDirs.append(root)

    allDirs.pop(0)
    return allDirs

if __name__ == '__main__':
    argsParser = argparse.ArgumentParser()

    argsParser.add_argument('-l', '--lexer', help='Start lexer tests', action='store_true')
    argsParser.add_argument('-p', '--parser', help='Start parser tests', action='store_true')
    argsParser.add_argument('-s', '--semantic', help='Start semantic tests', action='store_true')
    argsParser.add_argument('-g', '--generator', help='Star asm generator tests', action='store_true')
    argsParser.add_argument('-q', '--optimization', help='Start asm generator with optimizations test', action='store_true')

    args = argsParser.parse_args()

    if args.lexer:
        test(TESTS['-l'], ['-l'])

    if args.parser:
        test(TESTS['-p'], ['-p'])

    if args.semantic:
        test(TESTS['-s'], ['-p'])

    if args.generator:
        gen_test(GEN_TESTS['-g'], ['-g'])

    if args.optimization:
        gen_test(GEN_TESTS['-g'], ['-g', '-q'])

    if not args.lexer and not args.parser and not args.semantic and not args.generator:
        for key in TESTS:
            test(TESTS[key], [key])

        for key in GEN_TESTS:
            gen_test(GEN_TESTS[key], [key])
            