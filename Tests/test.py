import os
import argparse
import glob
import subprocess as subpr
from os.path import basename

TESTS_PATH = os.path.dirname(os.path.realpath(__file__))

PATH_TO_COMPILER = TESTS_PATH + os.sep + '..' + os.sep + 'Compiler-FPC' + os.sep + 'bin' + \
    os.sep + 'Debug' + os.sep + 'Compiler-FPC.exe'

TESTS = {
    '-l': [TESTS_PATH + os.sep +'Lexer'],
    '-p': [TESTS_PATH + os.sep +'Parser'],
}

def test(folders, optinons = []):
    for folder in folders:
        print(folder + '...', end='')

        inputFiles = getInputFiles(folder)
        outputFiles = getOutputFiles(folder)
        answerFiles = getAnswerFile(folder)

        countTests = 0
        for file in inputFiles:
            outFileName = os.path.splitext(file)[0] + '.out'
            subpr.run([PATH_TO_COMPILER, '-f', file, '-o', outFileName] + optinons)

            ansFileName = os.path.splitext(file)[0] + '.ans'
            if open(outFileName, 'r').read() != open(ansFileName, 'r').read():
                print('(' + str(countTests) + os.sep + str(len(inputFiles)) + ')')
                print('Not passed: ' + outFileName)
                exit(0)

            countTests += 1

        print('(' + str(countTests) + '/' + str(len(inputFiles)) + ')')

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

    args = argsParser.parse_args()

    if args.lexer:
        test(TESTS['-l'], ['-l'])

    if args.parser:
        test(TESTS['-p'], ['-p'])

    if not args.lexer and not args.parser:
        for key in TESTS:
            test(TESTS[key], [key])
            