import os
import argparse
import glob
import subprocess as subpr
from os.path import basename

PATH_TO_COMPILER = 'C:\\Users\\slast\\Documents\\Projects\\Compiler-FPC\\Compiler-FPC\\bin\\Debug\\Compiler-FPC.exe'

def test(folders, optinons = {}):
    for folder in folders:
        print(folder + '...', end='')

        inputFiles = getInputFiles(folder)
        outputFiles = getOutputFiles(folder)
        answerFiles = getAnswerFile(folder)

        countTests = 0
        for file in inputFiles:
            outFileName = os.path.splitext(file)[0] + '.out'
            subpr.run([PATH_TO_COMPILER, '-f', file, '-o', outFileName, '-l'])

            ansFileName = os.path.splitext(file)[0] + '.ans'
            if open(outFileName, 'r').read() != open(ansFileName, 'r').read():
                print('(' + str(countTests) + '/' + str(len(inputFiles)) + ')')
                print('Not passed: ' + outFileName)
                exit(0)

            countTests += 1

        print('(' + str(countTests) + '/' + str(len(inputFiles)) + ')')

    print('All tests passed!')    

def getInputFiles(folder):
    return glob.glob(folder + '\\*.in')

def getOutputFiles(folder):
    return glob.glob(folder + '\\*.out')

def getAnswerFile(folder):
    return glob.glob(folder + '\\*.ans')

def getAllDirs(testsPath):
    allDirs = []

    for root, dirs, files in os.walk(testsPath):
        allDirs.append(root)

    allDirs.pop(0)
    return allDirs

if __name__ == '__main__':
    argsParser = argparse.ArgumentParser()
    argsParser.add_argument('-l', '--lexer', help='Start lexer sests', action='store_true')

    args = argsParser.parse_args()

    testsPath = os.path.dirname(os.path.realpath(__file__))

    if args.lexer:
        test([testsPath + '\\Lexer'])
    else:
        test(getAllDirs(testsPath))
